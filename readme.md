## Adding a new secret code to your project

	Right-lick your project => Manage user secretes

Add a new secret code like:

```json
{
  "github": "NXYZPPWERLMK4"
}
```

## Tool for generating 2FA codes for Github

Just run the tool and generate a code

The github secret key can originally be obtained from here:
 https://github.com/settings/security?type=app#two-factor-summary
 
 > Click => Two-Factor methods: => Authenticator app => ... => Edit
 
 Either you scan the QR code now with your phone or you click on the link below to get the secret code/key:
 "You can use the >> setup key << to manually configure your authenticator app."
 Now right-click your VS project and click "Manage user secrets" and add the key there like:

	 {
	  "github": "NXYZPPWERLMK4"
	 }

User secrets are not persisted to git repository!