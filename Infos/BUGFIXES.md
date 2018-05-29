### Bug #1 ###

- Build stucks at closes (this because of the big quantity of log messages)
- Stop-debug only support up to 500 messages (if not builded game stucks at closing) (this very rudimentary way of getting logs)
    - Caused by: Using a List instead of a string builder to append new lines
    
### Bug #2 ###

- Block camera when Escape is pressed

### Bug #3 ###

- Keys stucks when Escape is pressed

### Bug #4 ###

- Stacktrace has a little bug where first line of ArgumentNullExceptions prints on the same line

### Bug #5 ###

- Unnamed bug reappeared (I have checked HiddenFlags and created a new scene (that bring the bug of old GPU))
    - Solved by: PedActions.cs