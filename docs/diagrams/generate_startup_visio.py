from __future__ import annotations

import html
import shutil
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

NS = "http://schemas.microsoft.com/office/visio/2012/main"
ET.register_namespace("", NS)
ET.register_namespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
TEMPLATE = Path(__import__("vsdx").__file__).parent / "media" / "media.vsdx"
VSDX = OUT / "TOTP-Manager-Startup-Decision-Tree.vsdx"
SVG = OUT / "TOTP-Manager-Startup-Decision-Tree.svg"

nodes = [
    ("start", "Start TOTP Manager", 8.5, 21.0, 3.2, 0.7, "start"),
    ("instance", "First app instance?", 8.5, 19.7, 3.0, 1.0, "decision"),
    ("existing", "Activate existing window\nand exit new process", 13.7, 19.7, 3.0, 0.9, "end"),
    ("splash", "Show splash window", 8.5, 18.2, 3.2, 0.8, "process"),
    ("bootstrap", "Create WPF app, load configuration,\nand build DI host", 8.5, 16.9, 4.2, 0.9, "process"),
    ("settings", "Settings loaded?", 8.5, 15.4, 3.0, 1.0, "decision"),
    ("fatal", "Log fatal error and\nshut down", 13.7, 15.4, 2.8, 0.9, "error"),
    ("prepare", "Prepare locked authorization session\n(no interactive prompt)", 8.5, 13.9, 4.2, 0.9, "security"),
    ("main", "Show locked main window\nand close splash", 8.5, 12.5, 3.8, 0.9, "security"),
    ("render", "Wait for MainWindow.ContentRendered", 8.5, 11.2, 4.0, 0.8, "process"),
    ("method", "Configured authorization?", 8.5, 9.7, 3.4, 1.1, "decision"),
    ("setup", "Password setup", 3.0, 8.0, 2.8, 0.8, "process"),
    ("password", "Password unlock screen", 8.5, 8.0, 3.1, 0.8, "security"),
    ("hello", "Owned Windows Hello prompt\n(main HWND supplied)", 14.0, 8.0, 3.5, 0.9, "security"),
    ("valid", "Credentials valid?", 8.5, 6.4, 2.8, 1.0, "decision"),
    ("helloresult", "Hello verified?", 14.0, 6.4, 2.8, 1.0, "decision"),
    ("fallback", "Remain locked\nRetry or use password", 14.0, 4.7, 3.0, 0.9, "error"),
    ("unlocked", "Authorization state: Unlocked", 8.5, 4.7, 3.8, 0.8, "success"),
    ("front", "Immediately restore, activate,\nand bring app to foreground", 8.5, 3.3, 4.1, 0.9, "success"),
    ("load", "Load vault content and accounts", 8.5, 1.9, 3.8, 0.8, "process"),
    ("ready", "Application ready", 8.5, 0.7, 3.0, 0.7, "end"),
]

edges = [
    ("start", "instance", ""), ("instance", "existing", "No"), ("instance", "splash", "Yes"),
    ("splash", "bootstrap", ""), ("bootstrap", "settings", ""), ("settings", "fatal", "No"),
    ("settings", "prepare", "Yes"), ("prepare", "main", ""), ("main", "render", ""),
    ("render", "method", ""), ("method", "setup", "Not configured"),
    ("method", "password", "Password"), ("method", "hello", "Windows Hello"),
    ("setup", "unlocked", "Created"), ("password", "valid", "Submit"),
    ("valid", "unlocked", "Yes"), ("valid", "password", "No"),
    ("hello", "helloresult", ""), ("helloresult", "unlocked", "Yes"),
    ("helloresult", "fallback", "Cancel / fail"), ("fallback", "password", "Fallback"),
    ("unlocked", "front", ""), ("front", "load", ""), ("load", "ready", ""),
]

palette = {
    "start": ("#6750A4", "#FFFFFF"), "end": ("#334155", "#FFFFFF"),
    "process": ("#E8EEF9", "#172033"), "decision": ("#FFF1C2", "#3B2F00"),
    "security": ("#DDD6FE", "#241E4E"), "success": ("#D9FBE5", "#103D24"),
    "error": ("#FDE2E2", "#591B1B"),
}

def cell(parent, name, value, formula=None):
    attrs = {"N": name, "V": str(value)}
    if formula:
        attrs["F"] = formula
    ET.SubElement(parent, f"{{{NS}}}Cell", attrs)

def shape_xml(shape_id, text, x, y, w, h, kind):
    shape = ET.Element(f"{{{NS}}}Shape", {"ID": str(shape_id), "Type": "Shape"})
    for name, value in (("PinX", x), ("PinY", y), ("Width", w), ("Height", h)):
        cell(shape, name, value)
    cell(shape, "LocPinX", w / 2, "Width*0.5"); cell(shape, "LocPinY", h / 2, "Height*0.5")
    fill, foreground = palette[kind]
    cell(shape, "FillForegnd", fill); cell(shape, "FillPattern", 1)
    cell(shape, "LineColor", "#60708A"); cell(shape, "LineWeight", 0.018)
    cell(shape, "Rounding", 0.12 if kind != "decision" else 0)
    chars = ET.SubElement(shape, f"{{{NS}}}Section", {"N": "Character"})
    row = ET.SubElement(chars, f"{{{NS}}}Row", {"IX": "0"})
    cell(row, "Color", foreground); cell(row, "Size", 0.15); cell(row, "Style", 1 if kind in ("start", "end", "success") else 0)
    para = ET.SubElement(shape, f"{{{NS}}}Section", {"N": "Paragraph"})
    prow = ET.SubElement(para, f"{{{NS}}}Row", {"IX": "0"}); cell(prow, "HorzAlign", 1)
    geo = ET.SubElement(shape, f"{{{NS}}}Section", {"N": "Geometry", "IX": "0"})
    points = [(0.5, 0), (1, 0.5), (0.5, 1), (0, 0.5), (0.5, 0)] if kind == "decision" else [(0, 0), (1, 0), (1, 1), (0, 1), (0, 0)]
    for ix, (px, py) in enumerate(points, 1):
        row = ET.SubElement(geo, f"{{{NS}}}Row", {"T": "RelMoveTo" if ix == 1 else "RelLineTo", "IX": str(ix)})
        cell(row, "X", px); cell(row, "Y", py)
    ET.SubElement(shape, f"{{{NS}}}Text").text = text
    return shape

def connector_xml(shape_id, source, target, label):
    sx, sy, sw, sh = source[2:6]; tx, ty, tw, th = target[2:6]
    dx, dy = tx - sx, ty - sy
    if abs(dx) > abs(dy):
        x1, y1 = sx + (sw / 2 if dx > 0 else -sw / 2), sy
        x2, y2 = tx - (tw / 2 if dx > 0 else -tw / 2), ty
    else:
        x1, y1 = sx, sy - (sh / 2 if dy < 0 else -sh / 2)
        x2, y2 = tx, ty + (th / 2 if dy < 0 else -th / 2)
    shape = ET.Element(f"{{{NS}}}Shape", {"ID": str(shape_id), "Type": "Shape", "NameU": "Connector"})
    cell(shape, "BeginX", x1); cell(shape, "BeginY", y1); cell(shape, "EndX", x2); cell(shape, "EndY", y2)
    cell(shape, "PinX", (x1+x2)/2); cell(shape, "PinY", (y1+y2)/2)
    cell(shape, "Width", abs(x2-x1)); cell(shape, "Height", abs(y2-y1)); cell(shape, "EndArrow", 4); cell(shape, "LineColor", "#64748B")
    geo = ET.SubElement(shape, f"{{{NS}}}Section", {"N": "Geometry", "IX": "0"})
    r1 = ET.SubElement(geo, f"{{{NS}}}Row", {"T": "MoveTo", "IX": "1"}); cell(r1, "X", 0); cell(r1, "Y", 0)
    r2 = ET.SubElement(geo, f"{{{NS}}}Row", {"T": "LineTo", "IX": "2"}); cell(r2, "X", abs(x2-x1)); cell(r2, "Y", y2-y1)
    if label: ET.SubElement(shape, f"{{{NS}}}Text").text = label
    return shape

def build_vsdx():
    shutil.copyfile(TEMPLATE, VSDX)
    page = ET.Element(f"{{{NS}}}PageContents", {"{http://www.w3.org/XML/1998/namespace}space": "preserve"})
    shapes = ET.SubElement(page, f"{{{NS}}}Shapes")
    by_id = {n[0]: n for n in nodes}
    sid = 1
    for node in nodes:
        shapes.append(shape_xml(sid, *node[1:])); sid += 1
    for source, target, label in edges:
        shapes.append(connector_xml(sid, by_id[source], by_id[target], label)); sid += 1
    page_bytes = ET.tostring(page, encoding="utf-8", xml_declaration=True)
    temp = VSDX.with_suffix(".tmp")
    with zipfile.ZipFile(VSDX, "r") as zin, zipfile.ZipFile(temp, "w", zipfile.ZIP_DEFLATED) as zout:
        for item in zin.infolist():
            data = page_bytes if item.filename == "visio/pages/page1.xml" else zin.read(item.filename)
            if item.filename == "visio/pages/pages.xml":
                data = data.replace(b"Page-1", b"Startup Decision Tree")
            zout.writestr(item, data)
    temp.replace(VSDX)

def build_svg():
    scale, margin = 58, 30
    width, height = 17 * scale + margin * 2, 22 * scale + margin * 2
    by_id = {n[0]: n for n in nodes}
    def pos(n): return margin + n[2]*scale, margin + (22-n[3])*scale
    parts = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">',
             '<rect width="100%" height="100%" fill="#F8FAFC"/>',
             '<defs><marker id="arrow" markerWidth="8" markerHeight="8" refX="7" refY="4" orient="auto"><path d="M0,0 L8,4 L0,8 z" fill="#64748B"/></marker></defs>',
             '<text x="30" y="34" font-family="Segoe UI" font-size="22" font-weight="700" fill="#172033">TOTP Manager — Startup Decision Tree</text>']
    for a, b, label in edges:
        na, nb = by_id[a], by_id[b]; x1,y1=pos(na); x2,y2=pos(nb)
        parts.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="#64748B" stroke-width="2" marker-end="url(#arrow)"/>')
        if label: parts.append(f'<text x="{(x1+x2)/2+5}" y="{(y1+y2)/2-5}" font-family="Segoe UI" font-size="11" fill="#475569">{html.escape(label)}</text>')
    for _, text, x, y, w, h, kind in nodes:
        cx,cy=margin+x*scale,margin+(22-y)*scale; ww,hh=w*scale,h*scale; fill,fg=palette[kind]
        if kind == "decision":
            parts.append(f'<polygon points="{cx},{cy-hh/2} {cx+ww/2},{cy} {cx},{cy+hh/2} {cx-ww/2},{cy}" fill="{fill}" stroke="#60708A" stroke-width="2"/>')
        else: parts.append(f'<rect x="{cx-ww/2}" y="{cy-hh/2}" width="{ww}" height="{hh}" rx="10" fill="{fill}" stroke="#60708A" stroke-width="2"/>')
        lines=text.split("\n"); base=cy-(len(lines)-1)*8
        for i,line in enumerate(lines): parts.append(f'<text x="{cx}" y="{base+i*17}" text-anchor="middle" dominant-baseline="middle" font-family="Segoe UI" font-size="12" font-weight="{700 if kind in ("start","end","success") else 400}" fill="{fg}">{html.escape(line)}</text>')
    parts.append('</svg>'); SVG.write_text("\n".join(parts), encoding="utf-8")

if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    build_vsdx(); build_svg()
    print(VSDX); print(SVG)
