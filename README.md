# AsyncVoidAnalyzer
Analyzer that checks if async void methods catch exceptions.

Much has been written on the topic of avoiding async void methods whenever possible. There are other analyzers to help enforce that practice. However, sometimes such methods are appropriate/necessary. In these cases, the important thing is that they are used wisely and do not allow exceptions to escape (the inability for the caller to handle exceptions is one of the main arguments against using async void in the first place).

This is currently focused on the simple case where an async void method awaits without any exception handling. It will not flag insufficient exception handling (e.g. missed exception types, re-throwing, or having additional non-awaiting code after the await expression that can throw exceptions). Lambdas are supported.
