# Define the list of directories to zip
$directories = @(
    "source\HI2UC\bin\Release\net48",
    "source\HIColorer\bin\Release\net48",
    "source\TightHyphen\bin\Release\net48",
    "source\NoRecursiveDash\bin\Release\net48",
    "source\LinesUnbreaker\bin\Release\net48"
)

# Define the output directory where the zip files will be saved
$outputDirectory = "C:\plugins\"

# Create the output directory if it doesn't exist
if (-not (Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory
}

# Loop through each directory in the list
foreach ($directory in $directories) {
    if (Test-Path -Path $directory) {
        # Find the first .dll file in the directory
        $dllFile = Get-ChildItem -Path $directory -Filter *.dll | Select-Object -First 1

        if ($dllFile) {
            # Get the name of the dll file (without extension) to use as the zip file name
            $zipFileName = [System.IO.Path]::GetFileNameWithoutExtension($dllFile.Name)

            # Define the zip file path
            $zipFilePath = Join-Path -Path $outputDirectory -ChildPath "$zipFileName.zip"

            # Zip all files in the directory except .pdb files
            $itemsToZip = Get-ChildItem -Path $directory -Exclude *.pdb
            Compress-Archive -Path $itemsToZip.FullName -DestinationPath $zipFilePath

            Write-Output "Zipped '$directory' to '$zipFilePath'"
        } else {
            Write-Output "No .dll file found in directory '$directory'"
        }
    } else {
        Write-Output "Directory '$directory' does not exist"
    }
}