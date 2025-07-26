# WordBreak Middleware Demo

This ASP.NET Core application demonstrates the WordBreak middleware in action.

## Running the Demo

1. Navigate to the demo directory:
   ```bash
   cd WordbreakMiddleware.Demo
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser to https://localhost:5001 (or the URL shown in the console)

## Features

### Interactive Configuration

The demo page (`/Demo`) allows you to:

- **Enable/Disable** the middleware in real-time
- **Adjust minimum character length** for processing
- **Select word break boundaries**:
  - Namespace (dots in fully qualified names)
  - PascalCase transitions
  - camelCase transitions
  - Number boundaries
  - ALL_CAPS transitions

### Sample Content

The demo includes various types of .NET identifiers:
- Simple class names (`HttpClient`, `DelegatingHandler`)
- Namespaced types (`System.Net.Http.HttpClient`)
- Mixed case with numbers (`HttpClient2MessageHandler3`)
- Acronyms (`HTTPSConnectionHandler`, `XMLParser`)
- Very long identifiers (Java-style factory names)

### Visual Feedback

Word break locations (`<wbr>` tags) are highlighted with a red background for demonstration purposes, making it easy to see exactly where breaks are being inserted.

## How It Works

The demo uses query string parameters to configure the middleware dynamically:

- `?wordbreak=on` - Enables the middleware
- `?minchars=15` - Sets minimum characters to 15
- `?boundaries=Namespace,PascalCase` - Enables specific boundaries

The middleware is applied conditionally based on these parameters, allowing you to test different configurations without restarting the application.

## Example URLs

Try these URLs to see different configurations:

- `/Demo?wordbreak=on&boundaries=Namespace` - Only break at namespace dots
- `/Demo?wordbreak=on&boundaries=PascalCase,Numbers` - Break at PascalCase and numbers
- `/Demo?wordbreak=on&minchars=30` - Only process very long identifiers
- `/Demo` - Middleware disabled (default)