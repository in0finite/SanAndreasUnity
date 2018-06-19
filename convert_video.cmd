@echo off
"D:/APPS/ffmpeg/bin/ffmpeg.exe" -y -r 30 -f image2pipe -vcodec ppm -i output.ppm  -b:v 3048780 -vcodec libx264 -crf 24 output.mp4
pause