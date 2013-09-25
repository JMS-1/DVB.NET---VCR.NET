#include "stdafx.h"
#include "SizeReport.h"
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#include <winsock.h>

#include <DVBFrontend.h>
#include <DVBBoardControl.h>
#include <DVBFilterToFile.h>
#include <DVBRemoteControl.h>
#include <DVBComnIF.h>
#include <DVBTeletext.h>

#define GENERATE(cls)	Generate(# cls, sizeof(cls))


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

void ReportBudget(void)
{
	// Run code
	CString *pNew = new CString(L"UNICODE");

	// Destory code
	delete pNew;

	// Header
	cout << "using System;" << endl;
	cout << endl;
	cout << "namespace JMS.DVB.Provider.TTBudget" << endl;
	cout << "{" << endl;
	cout << "\t/// <summary>" << endl;
	cout << "\t/// Holds the size of all TT API/ADK 2.19e C++ classes." << endl;
	cout << "\t/// </summary>" << endl;
	cout << "\tpublic static class LegacySize" << endl;
	cout << "\t{" << endl;
	
	// Process
	GENERATE(CDVBFrontend);	
	GENERATE(CDVBRemoteControl);
	GENERATE(CDVBTSFilter);
	GENERATE(CDVBFilterToFile);
	GENERATE(CDVBComnIF);
	GENERATE(CDVBTeletext);
	GENERATE(CString);
	GENERATE(CDVBBoardControl);

	// Done
	cout << "\t}" << endl;
	cout << "}" << endl;
}

