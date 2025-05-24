# --- Step 1: Remove existing reports ---
Write-Host "Cleaning existing reports..." -ForegroundColor Green
rm -r ./TestResults

Write-Host ""
# --- Step 2: Clean Build & Tests ---
Write-Host "Cleaning existing builds and test results..." -ForegroundColor Green
dotnet clean

Write-Host ""
# --- Step 3: Build & Restore ---
Write-Host "Build and restore project..." -ForegroundColor Green
dotnet build ./eKIBRA.sln `
  /property:GenerateFullPaths=true `
  /consoleloggerparameters:NoSummary

Write-Host ""
# --- Step 4: Run Tests & Collect Coverage ---
Write-Host "Running tests and collecting coverage..." -ForegroundColor Green
dotnet test ./eKIBRA.sln `
  --no-restore `
  --no-build `
  --results-directory ./TestResults/ReportGenerator `
  --collect:"XPlat Code Coverage" `
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

Write-Host ""
# --- Step 5: Generate Report with Additional Filtering ---
Write-Host "Generating coverage report..." -ForegroundColor Green
reportgenerator `
  -reports:./TestResults/ReportGenerator/*/coverage.cobertura.xml `
  -targetdir:./TestResults/ReportGenerator `
  -classfilters:"-eKIBRA.Web.Data.Migrations.*;-Program" `
  -filefilters:"-**/eKIBRA.Web.Data.Migrations/**;-**/Program.cs" `
  -riskhotspotclassfilters:"-Program" `
  -assemblyfilters:"-*.Tests"