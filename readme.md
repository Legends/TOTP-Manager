<!-- TOC-START (DO NOT REMOVE OR CHANGE COMMENT!) -->
**Table of Contents**

- [TOTP Manager App](#totp-manager-app)
  - [Build Status](#build-status)
  - [Features](#features)
  - [Usage](#usage)
<!-- TOC-END (DO NOT REMOVE OR CHANGE COMMENT!) -->

# TOTP Manager App

WPF app for managing TOTP secrets that generate TOTP codes and QR codes.
Uses `Otp.NET` for generating TOTP codes and `QRCoder` for generating QR codes.

<img src="https://i.imgur.com/KnXttHz.png" alt="TOTP Manager" Height="450">

## Build Status

![Build Status](https://github.com/Legends/TOTP-Code-Generator/actions/workflows/build-and-test.yml/badge.svg)

## Features

- Generate TOTP codes + QR codes
- CRUD TOTP secrets
- Store secrets securely

## Usage

- Generate a code: Click on an entry
- Edit: double-click on a field
- Delete: right-click the entry and select "Delete".

## How to run: Create executable `TOTP.Manager.exe`

Three ways how to create a running executable:

1. Download the source code folder from [Releases section](https://github.com/Legends/TOTP-Code-Generator/releases)

Open Powershell and `cd` to `TOTP-Manager-App` folder and:

Execute `publish.ps1` from inside the solution folder:

    .\publish.ps1

Create your own fully signed executable if you have valid official certificate for signing:

    .\publish.ps1 -CertPath ".\certs\my-cert.pfx" -CertPassword "mypassword"

2. Download and unzip the `TOTP.Manager.zip` file from the [Releases section](https://github.com/Legends/TOTP-Code-Generator/releases)

The zip file contains the unsigend `.exe` file.
Accept Smart Screen prompt, because we don't have a valid signing certificate:

<p align="center">
  <img src="https://i.imgur.com/wIef0Os.png" alt="Smart Screen prompt" height="200">
  <img src="https://i.imgur.com/6dz6fZa.png" alt="Smart Screen prompt" height="200">
</p>
