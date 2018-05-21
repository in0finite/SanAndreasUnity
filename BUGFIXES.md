### Bug #1 ###

- Build stucks at closes (this because of the big quantity of log messages)
- Stop-debug only support up to 500 messages (if not builded game stucks at closing) (this very rudimentary way of getting logs)
    - Caused by: Using a List instead of a string builder to append new lines