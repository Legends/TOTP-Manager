using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog;
using System;
using System.Windows;
using TOTP.Core.Services;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Security.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.Startup;
using TOTP.Views;

namespace TOTP;

public partial class App : Application
{
   
}