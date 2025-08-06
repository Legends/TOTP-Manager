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
