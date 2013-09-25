// SizeReport.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "SizeReport.h"
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// The one and only application object

CWinApp theApp;

using namespace std;

namespace
{
	void Generate(LPCSTR className, int instanzeSize)
	{
		// Show it
		cout << endl;
		cout << "\t\t/// <summary>" << endl;
		cout << "\t\t/// Size of C++ class " << className << " in bytes." << endl;
		cout << "\t\t/// </summary>" << endl;
		cout << "\t\tpublic const int " << className << " = " << instanzeSize << ";" << endl;
	}
}

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	// Uses MFC
	if ( !::AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0) ) return 1;

	// Report all
	::ReportPremium();
	::ReportBudget();

	// Wait
	::AfxMessageBox(_T("Done"));

	// Done
	return 0;
}

