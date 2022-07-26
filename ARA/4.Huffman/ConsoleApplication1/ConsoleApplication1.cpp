// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
#include <string>
#include <deque>
#include <vector>
#include <fstream>
#include<cstdlib>

using namespace std;

class BinReader {
public:
	ifstream file;
	char var = 0;
	int x = 8;
	BinReader(string newPath) {
		file.open(newPath, ios::binary);
	}
	~BinReader() {
		file.close();
	}

	char readByte() {
		char ch;
		file.read(&ch, 1);
		if (file.peek() == file.eof()) { //pogleda en znak naprej in ga prebere če je eof
			char konec;                  //ker mi drugace spodaj izpise se en zanak ki ni del besedila
			file.read(&konec, 1);;
		}
		return ch;
	}
	bool readBit() {
		if (x == 8) {
			file.read(&var, 1);;
			x=0;
		}
		return(var >> x++) & 1;
	}
	int readInt(){
		int integer;
		file >> integer;
		return integer; //naslednje prebrano stevilo
	}
	float readFloat(){
		int dec;
		file >> dec;
		return dec; //naslednje prebrano decimalno stevilo
	}
};

class BinWriter {
public:
	ofstream ofd;
	char var = 0;
	int x = 0;
	BinWriter(const char *out) {
		ofd.open(out, ios::binary);
	}
	~BinWriter(){
		if (x > 0 && x < 8) {
			int z = x;
			for (int i = 0; i < (8 - z); i++) { //na koncu izpise nicle, da dopolni zadnji byte do 8bitov
				writeBit(0);
			}
		}
		if (x>0)
			ofd.write(&var,1);

		ofd.close();
	}

	void writeBit(int b){
		if (x==8){
			ofd.write(&var, 1);
			x = 0; 
			var = 0;
		}
		var ^= (-b ^ var) & (1 << x);
		x++;
	}
	void writeByte(char b){
		ofd.write(&b, 1);
	}
	void writeInt(int i){
		ofd<<i;
		//ofd<<" ";
	}
	void writeFloat(int f) {
		ofd << f;
		//ofd << " ";
	}
};

struct Drevo {
	char crka;
	int st_ponovitev;
	Drevo* oce;
	Drevo* leviSin;
	Drevo* desniSin;
};

struct stejCrke {
	char crka;
	int st_ponovitev;
	Drevo* vrh_poddrevesa;
};

struct tabelaKod {
	char crka;
	int st_ponovitev;
	vector<bool> koda;
};

void quickSort(deque<stejCrke> &tabela, int levi, int desni) {

	int i = levi;
	int j = desni;
	int pivot = tabela[(desni + levi) / 2].st_ponovitev;
	while (j >= i) {
		while (tabela[i].st_ponovitev<pivot)
			i++;
		while (tabela[j].st_ponovitev>pivot)
			j--;
		if (j >= i) {
			swap(tabela[i], tabela[j]);
			i++;
			j--;
		}
	}
	//=====DELI=====
	if (levi < j) {
		quickSort(tabela, levi, j);//levi del
	}
	if (i < desni) {
		quickSort(tabela, i, desni);//desni del
	}
}

void zgradiDrevo(stejCrke last1, stejCrke last2, deque<stejCrke> &tabela, Drevo* &head) {
	stejCrke* novoVozlisce = new stejCrke();
	Drevo* vrh = new Drevo();

	if (last1.vrh_poddrevesa != NULL) {
		if (last2.vrh_poddrevesa != NULL) { //oba sta ze v drevesu
			vrh->st_ponovitev = (last1.st_ponovitev) + (last2.st_ponovitev);
			vrh->leviSin = last1.vrh_poddrevesa;
			vrh->desniSin = last2.vrh_poddrevesa;

			last1.vrh_poddrevesa->oce = vrh;
			last2.vrh_poddrevesa->oce = vrh;
		}
		else { // 1. je ze v drevesu
			Drevo* sin2 = new Drevo();
			sin2->crka = last2.crka;
			sin2->st_ponovitev = last2.st_ponovitev;

			vrh->st_ponovitev = (last1.st_ponovitev) + (sin2->st_ponovitev);
			vrh->leviSin = last1.vrh_poddrevesa;
			vrh->desniSin = sin2;

			sin2->oce = vrh;
			last1.vrh_poddrevesa->oce = vrh;
		}
	}
	else if (last2.vrh_poddrevesa!=NULL && last1.vrh_poddrevesa == NULL) {  // 2. je ze v drevesu
		Drevo* sin1 = new Drevo();
		sin1->crka = last1.crka;
		sin1->st_ponovitev = last1.st_ponovitev;

		vrh->st_ponovitev = (sin1->st_ponovitev) + (last2.st_ponovitev);
		vrh->leviSin = sin1;
		vrh->desniSin = last2.vrh_poddrevesa;

		sin1->oce = vrh;
		last2.vrh_poddrevesa->oce = vrh;
	}
	else if (last2.vrh_poddrevesa == NULL && last1.vrh_poddrevesa == NULL) {  // nova dva
		Drevo* sin1 = new Drevo();
		Drevo* sin2 = new Drevo();

		sin1->crka = last1.crka;
		sin1->st_ponovitev = last1.st_ponovitev;

		sin2->crka = last2.crka;
		sin2->st_ponovitev = last2.st_ponovitev;

		vrh->st_ponovitev = (sin1->st_ponovitev) + (sin2->st_ponovitev);
		vrh->leviSin = sin1;
		vrh->desniSin = sin2;

		sin1->oce = vrh;
		sin2->oce = vrh;
	}
	head = vrh;

	novoVozlisce->crka = NULL;
	novoVozlisce->st_ponovitev = vrh->st_ponovitev;
	novoVozlisce->vrh_poddrevesa = vrh;
	tabela.push_front(*novoVozlisce);
}

void ustvari_tabelo_kod(Drevo* head, vector<bool> pot, vector<tabelaKod*> &tabelaKode) {
	if (head == NULL)
		return;
	
	if (head->leviSin != NULL) {
		pot.push_back(0);
		ustvari_tabelo_kod(head->leviSin, pot, tabelaKode);
		pot.pop_back();
		pot.push_back(1);
		ustvari_tabelo_kod(head->desniSin, pot, tabelaKode);// idi desno
	}
	else{
		tabelaKod* tabela = new tabelaKod();
		tabela->crka = head->crka;
		tabela->st_ponovitev = head->st_ponovitev;
		tabela->koda = pot;

		tabelaKode.push_back(tabela);
	}
}

void dekodiranje(string pot) {
	Drevo* head = NULL;

	BinReader* beriByte = new BinReader(pot);
	BinWriter* pisi = new BinWriter("dekodirano.txt");

	int stCrk = beriByte->readInt(); // prebere stevilo crk
	//cout << "st crk: " << stCrk;

	deque<stejCrke> table;
	stejCrke* tabela = new stejCrke[stCrk];
	int stevecCrk = 0;
	for (int i = 0; i < stCrk; i++) {
		tabela[i].crka = beriByte->readByte();// prebere crko
		tabela[i].st_ponovitev = beriByte->readInt(); //prebere st ponovitev
		tabela[i].vrh_poddrevesa = NULL;

		//cout <<"berem: "<< tabela[i].crka << " " << tabela[i].st_ponovitev << endl;

		stevecCrk = stevecCrk + tabela[i].st_ponovitev;
		table.push_back(tabela[i]);
	}
	
	//=================== ZGRADI DREVO =================//
	while (table.size() != 1) {

		stejCrke last1 = table.front();
		//cout << last1.crka;
		table.pop_front();
		stejCrke last2 = table.front();
		//cout << " "<<last2.crka;
		table.pop_front();

		zgradiDrevo(last1, last2, table, head);
		quickSort(table, 0, table.size() - 1);
	}
	//===== BERI BIT IN SE PREMIKAJ PO DREVESU ======//
	Drevo* koren = head;
	int stejPrebrano = 0;
	while (stejPrebrano!=stevecCrk) {
		head = koren;
		//cout << " ";
		while (head->leviSin != NULL && head->desniSin != NULL) {//list drevesa
			if (beriByte->readBit() == 0) {//idi levo
				head = head->leviSin;
			}
			else {//idi desno
				head = head->desniSin;
			}
		}
		//cout << head->crka<<" ";
		pisi->writeByte(head->crka);
		stejPrebrano++;
	}

}

int main(int argc, char *argv[]){
	string izbira = argv[1];
	string datoteka = argv[2];
	if (argv[2] == NULL) {
		cout << "Huffman <kodiranje ali dekodiranje> <datoteka> " << endl;
		cout << "     <kodiranje ali dekodiranje>: izberite c za kodiranje in d za dekodiranje" << endl;
		cout << "     <datoteka>: pot do vhodne datoteke" << endl;
		return 0;
	}

	//====================== POSTOPEK DEKODIRANJA =====================//
	if (izbira == "d") {
		dekodiranje(datoteka.c_str());
	}
	//====================== POSTOPEK KODIRANJA =====================//
	else {

		deque<stejCrke> tabela;
		deque<stejCrke> zacetnaTabela;
		Drevo* head = NULL;

		//============================== STETJE CRK ============================//
		bool zeObstaja = false;
		//BinReader* beriByte = new BinReader("besedilo.txt");
		BinReader* beriByte = new BinReader(datoteka.c_str());

		while (!beriByte->file.eof()) {
			zeObstaja = false;
			stejCrke* stej = new stejCrke();
			stej->crka = beriByte->readByte();

			for (int i = 0; i < tabela.size(); i++) {
				if (tabela[i].crka == stej->crka) {
					zeObstaja = true;
					tabela[i].st_ponovitev++;
				}
				else {
					stej->st_ponovitev = 1;
					stej->vrh_poddrevesa = NULL;
				}
			}

			if (zeObstaja == false)
				tabela.push_back(*stej);
		}

		tabela[0].st_ponovitev++;//ker mi prvega ne steje

		quickSort(tabela, 0, tabela.size() - 1);

		for (int i = 0; i < tabela.size(); i++) {
			zacetnaTabela.push_back(tabela[i]);
			//cout << tabela[i].crka << endl;
		}
		int stCrk = tabela.size();

		while (tabela.size() != 1) {//dokler ni tabela prazna

			stejCrke last1 = tabela.front();
			tabela.pop_front();
			//cout << last1.crka;
			stejCrke last2 = tabela.front();
			//cout << " " << last2.crka;
			tabela.pop_front();

			zgradiDrevo(last1, last2, tabela, head);
			quickSort(tabela, 0, tabela.size() - 1);
		}
		vector <bool> pot;
		vector<tabelaKod*> tabelaKode;
		ustvari_tabelo_kod(head, pot, tabelaKode);

		for (int i = 0; i < tabelaKode.size(); i++) {
			cout << "crka: " << tabelaKode[i]->crka << " koda: ";
			for (int j = 0; j < tabelaKode[i]->koda.size(); j++)
				cout << tabelaKode[i]->koda[j];
			cout << endl;
		}

		BinReader* beri = new BinReader(datoteka.c_str());
		BinWriter* pisi = new BinWriter("output.txt");

		//========== TABELA PONOVITEV CRK =============//
		pisi->writeInt(stCrk);
		int t = 0;
		for (int t = 0; t < stCrk; t++) {
			pisi->writeByte(zacetnaTabela[t].crka);
			pisi->writeInt(zacetnaTabela[t].st_ponovitev);
			//cout << zacetnaTabela[t].crka << " " << zacetnaTabela[t].st_ponovitev << endl;
		}//prvih (stCrk*2 je tabela)

		//================ KODIRANJE ==================//
		while (!beri->file.eof()) {
			int i = 0;
			//cout << beri->readByte()<<" ";
			char crka = beri->readByte();
			while (crka != tabelaKode[i]->crka) {
				i++;
			}
			for (int j = 0; j < tabelaKode[i]->koda.size(); j++) {
				//cout << tabelaKode[i]->koda[j];
				pisi->writeBit(tabelaKode[i]->koda[j]);
			}
			//cout << " ";
		}
		delete pisi; //destruktor
	}
	system("pause");
	return 0;
}
