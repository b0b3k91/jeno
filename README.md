# Jeno
*Jeno is a CLI allows to running Jenkins jobs from your local machine.*


## Available commands

##### `jeno run`
Runs new Jenkins job. Command must be running from git repository related with running job. You can also pass parameters to your job from command line. All you have to do is writing them after `run` command. E.g. command `jeno run integrationTests=true version=1.0.0` runs job with checked _"integrationTests"_ checkbox and parameter _"version"_ set to _1.0.0_

##### `jeno set`
Change jeno configuration. To set new value of selected parameter just write it name and new value with colon between them (e.g. `jeno set userName:jDoe`). Parameter name parsing is case insensitive, that means `username:jDoe` or `USERNAME:jDoe` is equivalent of `userName:jDoe`. See [Configuration section](#Configuration) to get full list of configuration parameters.

##### `jeno config`
Shows program configuration written in yaml format.

##### `jeno help`
Displays all program features.

## Configuration

Jeno configuration is saved in _appsettings.json_ file. You can modify this file directly or by `jeno set` command. List of configuration parameters:
* **jenkinsUrl**: URL address of your Jenkins instance.

* **userName**: Your Jenkins login. It will be used by Jeno to running your jobs.

* **token**: Authentication token. Can be generated in configuration section on Jenkins.

* **password**: Password to your Jenkins account. Used only when Jenkins has turn on CSRF protection. You can left this field empty, then Jeno ask you for your password during `run` command execution. Password stored in configuration file is encrypted, that's why you have to use `set` command to set or change your password.

* **repository**: List of _*repository*_:_*pipeline*_ maps. This collection allows Jeno to relate your repository with proper Jenkins pipeline. By default here is only one map with _*default*_ word as a key. This pipeline is selected when chosen repository is not on _*repository*_ list. _*default*_ pipeline cannot be deleted. If you have only one working Jenkins pipeline, then pass your pipeline marked with _*default*_ key. Otherwise create new _*repository*_:_*pipeline*_ map. 