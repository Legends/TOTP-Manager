![Build Status](https://github.com/Legends/TOTP-Code-Generator/actions/workflows/build-and-test.yml/badge.svg)

<!-- TOC-START (DO NOT REMOVE OR CHANGE COMMENT!) -->
**Table of Contents**
- [TOTP Manager App](#totp-manager-app)
  - [Features](#features)
  - [Usage](#usage)
  - [Setup](#setup)
    - [1. Build from Source](#1-build-from-source)
    - [2. Use the Prebuilt Zip](#2-use-the-prebuilt-zip)
<!-- TOC-END (DO NOT REMOVE OR CHANGE COMMENT!) -->

---

# TOTP Manager

A WPF application for managing TOTP secrets, generating TOTP codes, and creating corresponding QR codes.

Powered by:

- **Otp.NET** – for TOTP code generation  
- **QRCoder** – for QR code creation  

<img src="https://i.imgur.com/VtJm8yP.png" alt="TOTP Manager" height="400">

## Features

- Generate TOTP codes and QR codes
- Create, read, update, and delete TOTP secrets
- Securely store secrets
- Search by platform name

## Usage

- **Add new platform:**  click on plus symbol
- **Generate TOTP:** click on datagrid row
- **Edit:** 
    * double-click a field for changing the account name only
    * right-click a datagrid row and select "Edit" for changing all fields
- **Delete:** right-click a datagrid row and select "Delete"

## Setup

You can create a runnable executable in two ways:

### 1. Build from Source

- Download the source code from the [Releases](https://github.com/Legends/TOTP-Code-Generator/releases) section.
- Open a PowerShell console, navigate to the `TOTP-Manager-App` folder, and run:

```powershell
.\publish.ps1
```

- To create a fully signed executable (requires a valid code-signing certificate), run:

```powershell
.\publish.ps1 -CertPath ".\certs\my-cert.pfx" -CertPassword "mypassword"
```

### 2. Use the Prebuilt Zip

- Download `TOTP.UI.WPF.zip` from the [Releases](https://github.com/Legends/TOTP-Code-Generator/releases) section.  
- Extract the ZIP file.  
- Run the `.exe` file.  
  > The executable is unsigned; you will need to accept the Windows SmartScreen prompt.

<p align="center">
  <img src="https://i.imgur.com/wIef0Os.png" alt="SmartScreen prompt" height="200">
  <img src="https://i.imgur.com/6dz6fZa.png" alt="SmartScreen prompt" height="200">
</p>
