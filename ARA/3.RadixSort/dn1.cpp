#include <iostream>
#include <string>
#include <vector>
#include <fstream>

using namespace std;


int main(int argc, char *argv[])
{
	string vhod_dat;
	vhod_dat = argv[1];

	cout << "argv[0] = " << argv[0] << endl;
	cout << "argv[1] = " << argv[1] << endl;

	int nrOf = 0;
	int stevilka;

	//prestejem stevilo stevil v datoteki da nastavim velikost arraya
	//ifstream vnos("stevilke.txt");
	ifstream vnos(vhod_dat.c_str());
	if (vnos.is_open()) {
		while (vnos >> stevilka) {
			nrOf++;
		}
	}
	else {
		cout << "Ni mogoce odpreti datoteke - " << vhod_dat << endl;
	}
	//cout << nrOf << endl;

	vector <unsigned char> num(nrOf);
	vector <unsigned char> endnr(nrOf);

	int sst;
	int index = 0;
	//V array num vnasam stevila iz datoteke
	//ifstream read("stevilke.txt");
	ifstream read(vhod_dat.c_str());
	if (read.is_open()) {
		while (read >> sst) {
			num[index] = sst;
			index++;
		}
	}
	else {
		cout << "Ni mogoce odpreti datoteke - " << vhod_dat << endl;
	}
	
	vector <bool> bits(nrOf);

	int k = 0;
	while (k<8) {
		//Vzamem k-ti bit
		for (int i = 0; i < nrOf; i++) {
			bits[i] = (num[i] >> k) & 1;
		}

		//COUNTING SORT
		//ker sta 2 bita je pomozni array velik 2
		vector <int> Carr(2);
		
		//indexe c arraya povecam za 1 glede na število posameznih bitov
		for (int i = 0; i<nrOf; i++) {
			Carr[bits[i]] = Carr[bits[i]] + 1;
		}

		//zaporedno pristejem
		Carr[1] = Carr[1] + Carr[0];

		//v endnr shranim številke po c arrayu, tako da je endnr polje urejenih zacetnih stevil po k-tem bitu
		for (int i = (nrOf - 1); i >= 0; i--) {
			endnr[Carr[bits[i]] - 1] = num[i];
			Carr[bits[i]] = Carr[bits[i]] - 1;
		}
		//num zamenjam z novo kreiranim endnr , in grem na naslednji bit
		for (int i = 0; i < nrOf;i++) {
			num[i] = endnr[i];
		}
		
		k++;
	}

	vector <int> sortedNum(nrOf);
	for (int o = 0; o < nrOf;o++) {
		sortedNum[o] = num[o];
		//cout << sortedNum[o] << endl;
	}
	//cout << endl;

	//Izpis v datoteko
	ofstream sorted("out.txt");
	for (int o = 0; o < nrOf; o++) {
		sorted << sortedNum[o] << " ";
	}

	cout << "Stevila so sortirana v datoteko out.txt\n";

	//Izpis char stevil binarno
	/*
	vector <bool> bb(nrOf);

	for (int p = 0; p < nrOf;p++) {
		for (int j = 0; j < 8; j++) {
			bb[j] = (num[p] >> j) & 1;
		}

		for (int s = 7; s >= 0; s--) {
			cout << bb[s];
		}
		cout << endl;
	}
	*/

    return 0;
}

