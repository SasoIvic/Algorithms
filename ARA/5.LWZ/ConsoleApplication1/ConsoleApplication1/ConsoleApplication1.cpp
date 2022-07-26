// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <iostream>
#include <string>
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
			file.read(&konec, 1);
		}
		return ch;
	}
	char peekByte() {
		return file.peek();
	}
	bool readBit() {
		if (x == 8){
			var = 0;
			x = 0;
		}
		if (var == 0){
			var = readByte();
		}
		bool bit = (var >> 7 - x) & 1;
		x++;
		return bit;
	}
	int readInt() {
		int integer;
		file >> integer;
		return integer; //naslednje prebrano stevilo
	}
	float readFloat() {
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
	~BinWriter() {
		if (x > 0 && x < 8) {
			int z = x;
			for (int i = 0; i < (8 - z); i++) { //na koncu izpise nicle, da dopolni zadnji byte do 8bitov
				writeBit(0);
			}
		}
		if (x>0)
			ofd.write(&var, 1);

		ofd.close();
	}

	void writeBit(int b) {
		if (x == 8){
			writeByte(var);
			x = 0; var = 0;
		}
		var ^= (-b ^ var) & (1 << 7 - x);
		x++;
	}
	void writeByte(char b) {
		ofd.write(&b, 1);
	}
	void writeInt(int i) {
		ofd << i;
		//ofd<<" ";
	}
	void writeFloat(int f) {
		ofd << f;
		//ofd << " ";
	}
};

string convertDecimalToBinary(int n, int size){
	long long binaryNumber=0;
	int ostanek;
	int i = 1;
	int step = 1;

	while (n != 0){
		ostanek = n % 2;
		n = n / 2;
		binaryNumber = binaryNumber + ostanek * i;
		i = i * 10;
	}
	string bin = to_string(binaryNumber);
	while(bin.length() != ceil(log2(size))) {
		bin = "0" + bin;
	}

	cout << " bin: " << bin << endl;
	return bin;
}

int convertBinaryToDecimal(long long n){
	int decimalNumber = 0;
	int i = 0;
	int ostanek;
	while (n != 0){
		ostanek = n % 10;
		n = n / 10;
		decimalNumber = decimalNumber + (ostanek * pow(2, i));
		++i;
	}
	return decimalNumber;
}

//=============================== KODIRANJE ===================================//
void kodiranje(vector<string> slovar, string trenutnaBeseda, string novaBeseda, vector<string> koda, int maxSize) {

	BinReader* beriByte = new BinReader("datoteka.txt");
	BinWriter* pisi = new BinWriter("out.txt");

	string naslednja;
	while (!beriByte->file.eof()) {
		/*if (slovar.size() > maxSize) {
			while (slovar.size() != 255) {
				slovar.pop_back();
			}
		}*/

		bool zeObstaja = false;
		trenutnaBeseda = beriByte->readByte();

		for (int k = 0; k < slovar.size(); k++) {
			if ((trenutnaBeseda + beriByte->peekByte()) == slovar[k]) { //dokler ne pride DO nove besede
				trenutnaBeseda = trenutnaBeseda + beriByte->readByte(); //dodaja crke besedi
			}
		}
		naslednja = beriByte->peekByte();
		novaBeseda = trenutnaBeseda + naslednja; //nova beseda za slovar
		cout << "nova: " << novaBeseda << endl;

		for (int j = 0; j < slovar.size(); j++) {
			if (trenutnaBeseda == slovar[j]) {
				cout << " i: " << j << endl;
				koda.push_back(convertDecimalToBinary(j, slovar.size()+1));
			}
		}
		for (int h = 0; h < slovar.size(); h++) {
			if (novaBeseda == slovar[h]) {
				zeObstaja = true;
			}
		}
		if (zeObstaja != true) {
			slovar.push_back(novaBeseda);
		}
	}

	for (int i = 0; i < koda.size(); i++) {//pisi
		for (int j = 0; j < koda[i].length(); j++) {
			cout << koda[i][j] << " ";
			if (koda[i][j] == '1')
				pisi->writeBit(1);
			else
				pisi->writeBit(0);
		}
		cout << endl;
	}

	delete pisi;
}

//=============================== DeKODIRANJE ===================================//
void dekodiranje(vector<string> slovar, string prebrana, string novaBeseda, int maxSize) {

	BinReader* beriByte = new BinReader("out.txt");
	BinWriter* pisi = new BinWriter("dekodirano.txt");

	int stej = 0;
	while (!beriByte->file.eof()) {
		/*if (slovar.size() > maxSize) {
			while (slovar.size() != 256) {
				slovar.pop_back();
			}
		}*/

		string index = "";
		string prejsnja = prebrana;
		
		for (int i = 0; i < (ceil(log2(slovar.size()+2))); i++) { //prebere potrebno stevilo bitov
			index += to_string(beriByte->readBit());
		}
		cout << "binary: " << index << " ";
		int st = convertBinaryToDecimal(stoll(index));//dobi index v slovarju

		cout << " stevilo: " << st << "\n";

		if (st <= slovar.size()) {
			for (int i = 0; i < slovar[st].length(); i++) {//zapise string
				pisi->writeByte(slovar[st][i]);
			}
			prebrana = slovar[st];
		}
		else { //CE JE NI V SLOVARJU ... PALINDROM
			string pal = prejsnja + prejsnja[0];
			slovar.push_back(pal);
			for (int i = 0; i < pal.length(); i++) {
				pisi->writeByte(pal[i]);
			}
		}

		if (stej != 0) {//pri prvi ne dodas nicesar
			bool zeObstaja = false;
			novaBeseda = prejsnja + prebrana[0];
			cout << "\nnova: " << novaBeseda << "   velkost slovarja: " << slovar.size() << endl;
			for (int i = 0; i < slovar.size(); i++) {//dodas le ce se ni v seznamu
				if (novaBeseda == slovar[i]) {
					zeObstaja = true;
				}
			}
			if (zeObstaja != true) {
				slovar.push_back(novaBeseda);
			}
		}
		stej++;
	}
}

int main()
{
	int izbira;

	vector<string> slovar;
	string trenutnaBeseda = "";
	string novaBeseda = "";
	vector<string> koda;
	int maxSize = 300;

	//============================== SLOVAR =======================================//
	for (int i = 0; i < 256; i++) {//vsi ASCII znaki
		char temp = i;
		slovar.push_back(string(&temp, 1));
	}

	/*for (int i = 65; i <= 90; i++) {//velike crke
	char temp = i;
	slovar.push_back(string(&temp, 1));
	}
	slovar.push_back(" ");*/

	cout << "Izberite kodiranje(1) ali dekodiranje(2)" << endl;
	cin >> izbira;

	if (izbira == 1)
		kodiranje(slovar, trenutnaBeseda, novaBeseda, koda, maxSize);
	else
		dekodiranje(slovar, trenutnaBeseda, novaBeseda, maxSize);

	system("pause");
}

