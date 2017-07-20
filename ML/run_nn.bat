@echo off
rem euclidean is 1
set METRIC=%1
set NUM_THREADS=%2
set DATA_PATH=%3

set PROG_PATH=.\bin\Debug\NNClassifier.exe

set INPUT_FILE=%DATA_PATH%/samples.csv
set WEIGHT_FILE=%DATA_PATH%/output/graph_nodes.csv
set OUTPUT_FILE_PATH=%DATA_PATH%/output/nn_classifications.csv

%PROG_PATH% %INPUT_FILE% %WEIGHT_FILE% %OUTPUT_FILE_PATH% %METRIC% %NUM_THREADS%

echo RAN Command: %PROG_PATH% %INPUT_FILE% %WEIGHT_FILE% %OUTPUT_FILE_PATH% %METRIC% %NUM_THREADS%
