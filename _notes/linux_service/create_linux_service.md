https://gist.githubusercontent.com/miromannino/a17f3e6f3fdcb4d94a1f/raw/e9bc2a8179860701224be896b420a635454dd3bd/service.md
# Sample service script for debian/ubuntu
Look at [LSB init scripts](http://wiki.debian.org/LSBInitScripts) for more information.

## Usage


Publish the app.
create folder /opt/ws_openjkn
create folder /opt/ws_openjkn/run
Copy release bin files into folder /opt/ws_openjkn
copy file script wsopenjkn into folder /opt/ws_openjkn


Copy file script wsopenjkn or wsopenjkn_service.sh  to `/etc/init.d`:

```sh

cp "wsopenjkn" "/etc/init.d/wsopenjkn"
chmod +x /etc/init.d/wsopenjkn
chown root /etc/init.d/wsopenjkn
chgrp root /etc/init.d/wsopenjkn
```


## Test the service

```sh
service wsopenjkn start
service wsopenjkn stop
```

## Run at boot-time

```sh
update-rc.d wsopenjkn defaults
```

## Uninstall

The service can uninstall itself with `service $NAME uninstall`. 
Yes, that's very easy, therefore a bit dangerous. 
But as it's an auto-generated script, you can bring it back very easily. 
I use it for tests and often install/uninstall, that's why I've put that here.

Don't want it? Remove lines 56-58 of the service's script.

## Logs?

Your service will log its output to `/var/log/$NAME.log`. Don't forget to setup a logrotate :)

## Problems

```unable to execute /etc/init.d/wsopenjkn: No such file or directory.```

Maybe it has been modified by a Windows's editor and the line endings could be not corrected. 
This can be verified with the command ```cat -v wsopenjkn_wrap.sh``` and verifying that there are some strange characters as ``^M``.

To fix that:

```sed -i 's/\r//g' wsopenjkn_wrap.sh```