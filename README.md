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

#### Re-creating UserData and TokenStore
If you've databased the connection details, you can recreate them as such and then pass them to the last constructor in the above example.
This also describes the minimum data that you are required to store.
```csharp
// Create the credential objects
UserDetails userDetails = new UserDetails
{
	DeviceId = Guid.Parse(guid),
	DeviceIdExpiry = DateTime.Now.AddHours(24),
	Email = emailAddress,
	UserId = userId,
};

TokenStore tokenStore = new TokenStore
{
	access_token = accessToken,
	CreatedDate = DateTime.Now,
};
```

#### OAuth token refreshing
If you plan to use the library in a service, you may wish to use the automatic token refresh
```csharp
JlrSharpConnection jlrSharp = new JlrSharpConnection("your@email.com", "your_password");
jlrSharp.AutoRefreshTokens = true;
```

### Retreiving vehicles

#### Get the default vehicle
GetPrimaryVehicle() will return the first car linked on the InControl app.
```csharp
// Returns the primary vehicle, as a generic vehicle
Vehicle vehicle = jlrSharp.GetPrimaryVehicle();
```

#### Determine if vehicle is fossil fueled or electrict
By converting to a more specific type will open vehicle specific actions. Some functionality is provided by the base class, such as GetMileage()
```csharp
if (vehicle is ElectricVehicle electricVehicle)
{
	...
}
else if (vehicle is GasVehicle gasVehicle)
{
	...
}
```

### Vehicle functions

#### Generic functions
```csharp
vehicle.GetMileage(); // Returns the odometer reading in miles
vehicle.GetServiceDueInMiles(); // Returns the miles until next service
vehicle.IsLocked(); // Determines if the vehicle is locked
```
The following functions require the pin code
```csharp
vehicle.Lock("1234");
vehicle.Unlock("1234");
vehicle.HonkAndBlink("1234");
```

#### GasVehicle specific functions
```csharp
vehicle.GetDistanceUntilEmpty(); // Returns the remaining range until empty
vehicle.GetFuelLevelPercentage(); // Returns the remaining fuel level as a percentage
vehicle.StartEngine(string pin);
vehicle.StopEngine(string pin);
```

#### ElectricVehicle specific functions
```csharp
vehicle.IsPluggedIn(); // Determines if the vehicle is plugged in
vehicle.IsCharging(); // Determines if the vehicle is charging
vehicle.StartCharging(string pin); // Instructs the car to start charging
vehicle.StopCharging(string pin); // Instructs the car to stop charging
vehicle.StartClimatePreconditioning(string pin, string targetTemp); // Instructs the vehicle to start pre-conditioning the vehicle
vehicle.StopClimatePreconditioning(string pin); // Instructs the vehicle to start pre-conditioning the vehicle
```