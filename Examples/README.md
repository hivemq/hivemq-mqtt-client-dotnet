
## How to Wait on an Event

Use a `TaskCompletionSource` in your event handlers to wait for events to complete.

```c#
// The TaskCompletionSource setup
var taskCompletionSource = new TaskCompletionSource<bool>();

// Event handler that sets the result of the `TaskCompletionSource`
client.AfterDisconnect += (sender, args) =>
{
    // Do the AfterDisconnect work for your application
    //
    // Mark the taskCompletionSource as completed
    taskCompletionSource.SetResult(true);
};

// Connect
var result = await client.ConnectAsync().ConfigureAwait(false);
//
// Insert application work here...
//
// and Disconnect
await client.DisconnectAsync().ConfigureAwait(false);

// Wait for the AfterDisconnect event handler to finish
// ...with a 5 second timeout as a hang safety
await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
```
