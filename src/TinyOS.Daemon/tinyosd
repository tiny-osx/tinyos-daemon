#!/bin/sh
# /etc/init.d/tinyosd

### BEGIN INIT INFO
# Provides:          tinyosd
# Required-Start:    $local_fs $network  
# Required-Stop:     $local_fs $network
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: Start debugging daemon at boot time
# Description:       Enable vsdbg network debugging provided by tinyosd daemon. 
### END INIT INFO

set -e

RUNAS=root
PIDFILE=/var/run/tinyosd.pid
DAEMON="/usr/share/tinyosd/tinyosd"

test -x $DAEMON || exit 0

start() {  
  start-stop-daemon --start \
   --background \
   --oknodo \
   --user $RUNAS \
   --make-pidfile \
   --pidfile $PIDFILE \
   --exec $DAEMON
}

stop() {  
  start-stop-daemon --stop \
   --oknodo \
   --retry 5 \
   --name tinyosd \
   --pidfile $PIDFILE
  
  rm -f $PIDFILE
}

status(){
  if [ -f $PIDFILE ]; then
      PID="$(cat $PIDFILE)"
      if kill -0 $PID &>/dev/null; then
          echo "TinyOS debugger daemon is running with pid $(cat "$PIDFILE")"
          exit 0
      fi
  fi

  echo "TinyOS debugger daemon is not running"
  exit 1;
}

uninstall() {
  stop
  rm -f "$PIDFILE"
  update-rc.d -f tinyosd remove
  rm -fv "$0"
}

case "$1" in
  start)
    start
    ;;
  stop)
    stop
    ;;
  status)
    status
    ;;
  uninstall)
    uninstall
    ;;
  *)
    echo "Usage: $0 {start|stop|status|uninstall}"
    exit 1
esac

exit $?