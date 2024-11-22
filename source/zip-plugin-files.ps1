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
if (-not (Test-Path -Path $outputDirectory))
{
    New-Item -ItemType Directory -Path $outputDirectory
}

# Loop through each directory in the list
foreach ($directory in $directories)
{
    if (Test-Path -Path $directory)
    {
        # Find the first .dll file in the directory
        $dllFile = Get-ChildItem -Path $directory -Filter *.dll | Select-Object -First 1
        Copy-Item -Path $dllFile.FullName -Destination $outputDirectory
    }
    else
    {
        Write-Output "Directory '$directory' does not exist"
    }
}