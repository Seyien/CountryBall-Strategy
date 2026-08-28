# Testleri Unity Editor'e dokunmadan, komut satirindan calistirir.
#
# NEDEN VAR: test sonucunu ogrenmek icin birinin Editor'e gecip "Run All"a
# basmasini beklemek akisi kesiyordu.
#
# SINIR: Unity bir projeyi ayni anda tek bir ornekte acar. Editor acikken bu
# script calisamaz — bu bir hata degil, Unity'nin kilit mekanizmasidir.
#
# Kullanim:
#   powershell -File Tools\run-editmode-tests.ps1
#   powershell -File Tools\run-editmode-tests.ps1 -Filter "HealthTests"
#   powershell -File Tools\run-editmode-tests.ps1 -Category "Allocation"
#   powershell -File Tools\run-editmode-tests.ps1 -AssemblyNames "GridStrategy.Combat.EditModeTests"
#
# FILTRE SOZDIZIMI — WILDCARD DEGIL, REGEX (olculdu):
#   "HealthTests"          -> capasiz eslesme; adinda gecen HER testi alir
#   "^GridStrategy\.Tests\.EditMode\.Combat\.HealthTests\.Foo$"  -> tam olarak bir test
#   "A;B"                  -> A VEYA B
#   "!A"                   -> A haric
#   "*Foo*"                -> HATA: ArgumentException. Yildiz regex'te jokerdir,
#                             wildcard degil. Joker istiyorsan "." kullan.
#   [TestCase] parantezleri kacirilmali: "Foo\(100,10\)"

[CmdletBinding()]
param(
    [string]$UnityExe = "",
    [ValidateSet("EditMode", "PlayMode")]
    [string]$TestPlatform = "EditMode",
    [string]$Filter = "",
    [string]$Category = "",
    [string]$AssemblyNames = ""
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$artifactDir = Join-Path $projectPath "Tools\.test-results"
$resultsXml = Join-Path $artifactDir "$TestPlatform-results.xml"
$logFile = Join-Path $artifactDir "$TestPlatform-unity.log"

# SURUM ELLE YAZILMIYOR, PROJEDEN OKUNUYOR — ve gerekcesi olculdu: elle yazilan
# bir varsayilan bir gun projenin surumunden ayrilir, ve o gun kapi kirmizi
# vermez. Eski editor diskte durmaya devam ettigi icin script hatasizca ESKI
# Unity'yi acar ve YANLIS YESIL uretir: testler gecmis gibi gorunur ama olculen
# sey yukseltilmis proje degildir. (olculdu: bu proje 2021.3.45f2 -> 6000.5.7f1
# yukseltmesinde tam olarak bu tuzagi tasiyordu.)
if (-not $UnityExe) {
    $versionFile = Join-Path $projectPath "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path $versionFile)) { Write-Error "ProjectVersion.txt bulunamadi: $versionFile" }
    $versionLine = Select-String -Path $versionFile -Pattern '^m_EditorVersion:\s*(\S+)' | Select-Object -First 1
    if (-not $versionLine) { Write-Error "ProjectVersion.txt icinde m_EditorVersion satiri yok: $versionFile" }
    $editorVersion = $versionLine.Matches[0].Groups[1].Value
    $UnityExe = "C:\Program Files\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe"
    Write-Output "Projenin surumu: $editorVersion"
}

Write-Output "Kullanilan Unity: $UnityExe"
if (-not (Test-Path $UnityExe)) { Write-Error "Unity bulunamadi: $UnityExe" }

# Kilit kontrolu ONCE: Unity'yi bosuna baslatip crash handler'a dusmekten iyi.
$lockFile = Join-Path $projectPath "Temp\UnityLockfile"
if (Test-Path $lockFile) {
    Write-Output "BLOCKED: proje su anda Unity Editor'de acik (Temp\UnityLockfile mevcut)."
    Write-Output "Editor acikken en hizli yol Test Runner penceresinden calistirmaktir."
    exit 2
}

New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
if (Test-Path $resultsXml) { Remove-Item $resultsXml -Force }

$unityArgs = @(
    "-runTests", "-batchmode", "-nographics",
    "-projectPath", $projectPath,
    "-testPlatform", $TestPlatform,
    "-testResults", $resultsXml,
    "-logFile", $logFile
)
if ($Filter)        { $unityArgs += @("-testFilter", $Filter) }
if ($Category)      { $unityArgs += @("-testCategory", $Category) }
if ($AssemblyNames) { $unityArgs += @("-assemblyNames", $AssemblyNames) }

$scope = @()
if ($Filter)        { $scope += "filter='$Filter'" }
if ($Category)      { $scope += "category='$Category'" }
if ($AssemblyNames) { $scope += "assembly='$AssemblyNames'" }
if ($scope.Count -eq 0) { $scope = @("tumu") }

Write-Output "Kosuluyor: $TestPlatform | $($scope -join ' ')"
$proc = Start-Process -FilePath $UnityExe -ArgumentList $unityArgs -PassThru -Wait
Write-Output "Unity cikis kodu: $($proc.ExitCode)"

if (-not (Test-Path $resultsXml)) {
    Write-Output "SONUC XML URETILMEDI. Log: $logFile"
    exit 3
}

# XML tek gercek kaynak: konsol satiri degil, bu dosya sayilir.
[xml]$xml = Get-Content $resultsXml
$run = $xml.'test-run'
$total = [int]$run.total
$failed = [int]$run.failed

Write-Output ""
Write-Output "total=$total  passed=$($run.passed)  failed=$failed  skipped=$($run.skipped)  duration=$($run.duration)s"

# BOS KOSU KORUMASI: filtre hicbir testle eslesmezse total=0, failed=0 gelir ve
# script "basarili" gibi cikardi — yani yanlis yesil. Sifir test kosmasi asla
# basari degildir; ya filtre yanlis yazildi ya da testler kesfedilmedi.
if ($total -eq 0) {
    Write-Output ""
    Write-Output "HIC TEST KOSMADI. Bu bir BASARI DEGIL."
    if ($scope -ne "tumu") {
        Write-Output "Filtre hicbir testle eslesmemis olabilir. Hatirla: -Filter WILDCARD degil REGEX'tir;"
        Write-Output "'*Foo*' hata verir, capasiz 'Foo' zaten icinde-gecen eslesmesi yapar."
    }
    Write-Output "Log: $logFile"
    exit 4
}

if ($failed -gt 0) {
    Write-Output ""
    Write-Output "--- BASARISIZ TESTLER ---"
    $xml.SelectNodes("//test-case[@result='Failed']") | ForEach-Object {
        Write-Output "[FAIL] $($_.fullname)"
        if ($_.failure.message) { Write-Output "       $($_.failure.'#text' -replace '\s+', ' ')" }
        if ($_.failure.'stack-trace') {
            $first = ($_.failure.'stack-trace'.'#text' -split "`n" | Select-Object -First 1).Trim()
            if ($first) { Write-Output "       $first" }
        }
    }
    Write-Output ""
    Write-Output "XML: $resultsXml"
    exit 1
}

Write-Output ""
Write-Output "XML: $resultsXml"
exit 0
