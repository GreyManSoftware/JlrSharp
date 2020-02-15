# JlrSharp
A C# wrapper for the JLR InControl remote connectivity. You need a Jaguar Remote account with a valid subscription.

## Example Usage

### Connecting
Connecting can be done using username and password, or by caching off the respective OAuth tokens and using them for the next connection.

#### Using credentials
```csharp
// Connect to the Jaguar Remote service with your credentials
JlrSharpConnection jlrSharp = new JlrSharpConnection("your@email.com", "your_password");
```
#### Using OAuth tokens
```csharp
// Make the first connection as normal
JlrSharpConnection jlrSharp = new JlrSharpConnection("your@email.com", "your_password");

// Now, cache the credentials
AuthorisedUser credentials = jlrSharp.GetAuthorisedUser();

// The next connection
JlrSharpConnection jlrSharpNew = new JlrSharpConnection(credentials.UserInfo, credentials.TokenData);
```
#### OAuth token refreshing
If you plan to use the library in a service, you may wish to use the automatic token refresh
```csharp
JlrSharpConnection jlrSharp = new JlrSharpConnection("your@email.com", "your_password");
jlrSharp.AutoRefreshTokens = true;
```
You can only force a refresh of the tokens using this c'tor
```csharp
JlrSharpConnection jlrSharp = new JlrSharpConnection("your@email.com", refreshToken, deviceId);
```
### Retreive the default vehicle
```csharp
// Retrieve the primary vehicle
Vehicle vehicle = jlrSharp.GetPrimaryVehicle();
```
### Functions
```csharp
vehicle.GetMileage(); // Returns the odometer reading in miles
vehicle.GetServiceDueInMiles(); // Returns the miles until next service
vehicle.GetDistanceUntilEmpty(); // Returns the remaining range until empty
vehicle.GetFuelLevelPercentage(); // Returns the remaining fuel level as a percentage
```
### Functions that require your pin code
```csharp
vehicle.StartEngine("1234");
vehicle.StopEngine("1234");
vehicle.Lock("1234");
vehicle.Unlock("1234");
vehicle.HonkAndBlink("1234");
```
