# Jeno
*Jeno is a CLI allows to running Jenkins jobs from your local machine.*


## Available commands
---

### `jeno run`
Runs new Jenkins job. Command must be running from git repository related with running job. You can also pass parameters to your job from command line. All you have to do is writing them separated spaces after `run` command. E.g. command `jeno run integrationTests=true version=1.0.0` runs job with checked _"integrationTests"_ checkbox and parameter _"version"_ set to _1.0.0_

### `jeno set`
Change jeno configuration. To set new value of selected parameter just write it name and new value with colon between them (e.g. `jeno set userName:jDoe`). Parameter name parsing is case insensitive, that means `username:jDoe` or `USERNAME:jDoe` is equivalent of `userName:jDoe`. See [Configuration section](#Configuration) to get full list of configuration parameters.

### `jeno config`
Shows program configuration written in yaml format.

### `jeno help`
Displays all program features.

## Configuration