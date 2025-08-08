<!-- TOC-START (DO NOT REMOVE OR CHANGE COMMENT!) -->
- [TOTP Manager App](#totp-manager-app)
  - [Build Status](#build-status)
  - [Features](#features)
  - [Usage](#usage)
  - [Setup `TOTP.Manager`](#setup-totpmanager)
<!-- TOC-END (DO NOT REMOVE OR CHANGE COMMENT!) -->

---

# TOTP Manager App

A WPF application for managing TOTP secrets, generating TOTP codes, and creating corresponding QR codes.

Powered by:

- **Otp.NET** – for TOTP code generation  
- **QRCoder** – for QR code creation  

<img src="https://i.imgur.com/KnXttHz.png" alt="TOTP Manager" height="400">

## Build Status

![Build Status](https://github.com/Legends/TOTP-Code-Generator/actions/workflows/build-and-test.yml/badge.svg)

## Features

- Generate TOTP codes and QR codes
- Create, read, update, and delete TOTP secrets
- Securely store secrets

## Usage

- **Generate a code** – click an entry  
- **Edit** – double-click a field  
- **Delete** – right-click an entry and select **Delete**  

## Setup `TOTP.Manager`

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

- Download `TOTP.Manager.zip` from the [Releases](https://github.com/Legends/TOTP-Code-Generator/releases) section.  
- Extract the ZIP file.  
- Run the `.exe` file.  
  > The executable is unsigned; you will need to accept the Windows SmartScreen prompt.

<p align="center">
  <img src="https://i.imgur.com/wIef0Os.png" alt="SmartScreen prompt" height="200">
  <img src="https://i.imgur.com/6dz6fZa.png" alt="SmartScreen prompt" height="200">
</p>
