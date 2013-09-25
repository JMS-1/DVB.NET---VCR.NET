Option Explicit

Dim myPath
myPath = WScript.ScriptFullName
Dim myDir
myDir = Left(myPath, InStrRev(myPath, "\"))

Dim bat
bat = myDir & "Demux.bat"

Dim shell
Set shell = CreateObject("WScript.Shell")
Dim envColl
Set envColl = shell.Environment("Process")

Dim fileCount
fileCount = CLng(envColl("PlannedFiles"))

Dim duration
duration = CLng(envColl("Duration"))
If (duration >= 5) Then
	Dim station
	station = envColl("Station")
	If ((station <> "LIVE") And (station <> "EPG") And (station <> "PSI")) then
		Dim tool
		tool = """" & Replace(shell.CurrentDirectory, """", """""") & "\..\..\Tools\FTPWrap.exe"" /BAT=""" & Replace(bat, """", """""") & """ "

		Dim exec(1000)
		Dim index
		index = 0

		Dim i
		For i = 1 To fileCount
			Dim fmt
			fmt = envColl("PlannedFormat" & CStr(i))
			
			If (fmt = "2") Then
				Dim file
				file = envColl("PlannedFile" & CStr(i))
				
				Dim demux
				demux = tool & """" & Replace(file, """", """""") & """"
			
				Set exec(index) = shell.Exec(demux)
			
				index = index + 1
			End If
		Next

		While (index > 0)
			index = index - 1
			
			While (exec(index).Status = 0)
				WScript.Sleep 500
			WEnd
		WEnd
	End If
End If

WScript.Sleep 1000

        
