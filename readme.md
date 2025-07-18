## Windows Icon Cache Not Refreshed

Windows caches icons aggressively. Try this first:

🔄 Refresh Windows Explorer icon cache
Press Ctrl + F5 in the folder where your .exe is.

If that doesn’t help:

Open cmd.exe as Administrator.

Run:
 
	ie4uinit.exe -ClearIconCache
	taskkill /IM explorer.exe /F
	start explorer.exe

Or delete icon cache manually:

	del %localappdata%\IconCache.db /a