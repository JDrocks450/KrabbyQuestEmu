@echo off
set blenderPath = %1
set localPath = %0
set all = %*
ECHO all: %all%
ECHO path: %blenderPath%
ECHO localPath: %localPath%
CD %blenderPath%
cmd \k