@echo off
set NUM_CLUSTERS=%1
rem euclidean is 1
set METRIC=%2
set NUM_THREADS=%3
set DATA_PATH=%4

set PROG_PATH=.\bin\Debug\AggClustering.exe

set INPUT_FILE=%DATA_PATH%/graph_nodes.csv
set OUTPUT_FILE_PATH=%DATA_PATH%/agg_output.csv

%PROG_PATH% %INPUT_FILE% %OUTPUT_FILE_PATH% %NUM_CLUSTERS% %METRIC% %NUM_THREADS%

echo RAN Command: %PROG_PATH% %INPUT_FILE% %OUTPUT_FILE_PATH% %NUM_CLUSTERS% %METRIC% %NUM_THREADS%
