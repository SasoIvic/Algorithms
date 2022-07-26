// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <algorithm>

using namespace std;

//TABELA
void prefixi_sufixi(string *niz, int dolinaNiza, int *A) {
		int len = 0;
		A[0] = 0; 

		// the loop calculates lps[i] for i = 1 to M-1
		for (int i = 1; i < dolinaNiza; i++) {
			if (niz[i] == niz[len]) { //ce je na i-tem mestu enak crka kot na kerem drugem
				len++;
				A[i] = len;
			}
			else { // ko ni ponovitev crke v nizu
				if (len == 0) {
					A[i] = 0;
				}
				else {
					len = A[len - 1];
					i--;
				}
			}
		}				
}

void search(string niz, vector<char> *line) {
	int dolzinaNiza = niz.length();
	int dolzinaBesedila = line->size();

	int *A = new int[dolzinaNiza]; // najdaljsi prefix in sufix

	prefixi_sufixi(&niz, dolzinaNiza, A);

	for (int i = 0; i < 12; i++) {
		cout << A[i] << " ";
	}


}

int main()
{
	ifstream file("primer_besedilo.txt");
	vector<char> line;
	char vrstica;
	while (file >> vrstica) { //shrani besedilo v vektor
		line.push_back(vrstica);
	}
	/*for(int i=0; i<line.size(); i++)
		cout << line[i]<<" ";
	*/
	string niz;
	cout << "Vpisite niz ki ga iscete." << endl;
	cin >> niz;

	search(niz, &line);


    return 0;
}

