#include<iostream>
#include<string>
#include<stdlib.h>
#include<fstream>
#include<vector>
#include<deque>
#include<chrono>

using namespace std;

struct Vozlisce {
	int predhodnik;
	int dolzina;
	int status;
	int indeks;
	string ime;
};
void meni(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "1 ... PREBERI GRAF\n";
	cout << "2 ... ISKANJE V GLOBINO (OD VOZLISCA s DO VOZLISCA d)\n";
	cout << "3 ... ISKANJE V SIRINO (OD VOZLISCA s DO VOZLISCA d)\n";
	cout << "4 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

void beriGraf(Vozlisce*& p_poljeVozlisc, int st_vozlisc, vector<string> imena, vector<int> price, int** matrika){

    Vozlisce* poljeVozlisc=new Vozlisce[st_vozlisc];
    Vozlisce* p_vozlisce = poljeVozlisc;

	int k=0; //stetje vozlisc
	bool enaka;
	for(int i=0; i<imena.size(); i++){
        enaka=false;
		for(int j=i+1; j<imena.size(); j++){
			if(imena[i]==imena[j]){
				enaka=true;
			}
		}
		if(enaka == false){
			poljeVozlisc[k].ime=imena[i];
			poljeVozlisc[k].indeks=k;
			k++;
		}
	}
    p_poljeVozlisc=p_vozlisce;

    int i=0;
    int index1=0;
    int index2=0;
	while(i<imena.size()){
		index1=0;
		index2=0;
		for(int j=0; j<st_vozlisc; j++){
			if(imena[i]==poljeVozlisc[j].ime){ //enaki imeni
				index1=poljeVozlisc[j].indeks;
			}
			if(imena[i+1]==poljeVozlisc[j].ime){
				index2=poljeVozlisc[j].indeks;
			}
		}
		matrika[index1][index2] = price[i/2];
		matrika[index2][index1] = price[i/2];
		i=i+2;
	}

	/*cout<<"MATRIKA:"<<endl;
	for(int i=0; i<st_vozlisc; i++){
		for(int j=0; j<st_vozlisc; j++){
			cout<<matrika[i][j]<<"  ";
		}
		cout<<endl;
	}*/
}

Vozlisce* isciVozlisce(string IskanoVozlisce, int st_vozlisc, Vozlisce* p_poljeVozlisc){
	for (int i=0; i<st_vozlisc; i++){
		if (IskanoVozlisce==p_poljeVozlisc[i].ime){
                //cout<<p_poljeVozlisc[i].ime;
			return &p_poljeVozlisc[i];
		}
	}
	return NULL;
}

void izpisPoti(Vozlisce* p_vozlisc, Vozlisce* s, Vozlisce* v){
    if (v==s)//enako zacetno in koncno vozlisce
        cout<<"Pot je: "<<v->ime<<" ";
    else{
        if (v->predhodnik==-1)
			cout<<"Med "<<s->ime<<" in "<<v->ime<<" ni poti.";
		else{
			izpisPoti(p_vozlisc, s, &p_vozlisc[v->predhodnik]);
			cout<<v->ime<<" ";
		}
    }
}
void iskanje_v_globino_sirino(Vozlisce* s, Vozlisce* d, int** matrika, Vozlisce* p_vozlisc, int st_vozlisc, bool globina){
    //1-nepregledano, 2-vObdelavi, 3-razvito
    vector<Vozlisce*> sosedi;

    for(int i=0; i<st_vozlisc; i++){
        p_vozlisc[i].status=1; //nepregledano
        p_vozlisc[i].predhodnik=-1;
    }
    s->status=2; //vObdelavi
    s->dolzina=0;
    s->predhodnik=-1;

    deque<Vozlisce*> sklad;

    sklad.push_back(s);

    Vozlisce* v = NULL;
    while(!sklad.empty()){
        if(globina==true){//ISKANJE V GLOBINO
            v=sklad.back();
            sklad.pop_back();
        }
        else{ // ISKANJE V SIRINO
            v=sklad.front();
            sklad.pop_front();
        }
        if(v==d){
            izpisPoti(p_vozlisc, s, v);
            cout<<endl;
            return;
        }
        for(int i=0; i<st_vozlisc; i++){//dobi sosede
            if(matrika[v->indeks][i]!=0){
                Vozlisce *sosed = &p_vozlisc[i];
                sosedi.push_back(sosed);
            }
        }
        for (int i=0; i < sosedi.size(); i++){
             if(sosedi[i]->status==1){ //nepregledano
                sosedi[i]->status=2;
                sosedi[i]->dolzina=v->dolzina+1;
                sosedi[i]->predhodnik=v->indeks;
                sklad.push_back(sosedi[i]);
            }
        }
        sosedi.clear();
        v->status=3;
    }
}

int main()
{
    Vozlisce *p_poljeVozlisc = NULL;
    int **matrika;
    int st_vozlisc;
    int st_povezav;
    string ime1;
    string ime2;
    int cena;
    vector<int> price;
    vector<string> imena;

    bool grafNalozen=false;

    bool running=true;
    do{
        int izbira;
        meni();
        cin>>izbira;

        if(izbira==1){

            cout<<"Kateri graf nalozim?"<<endl;
             cout<<"    0...grafBig.txt";
             cout<<"    1...graf.txt";
             cout<<"    2...grafImena.txt"<<endl;
            int izberiGraf;
            cin>>izberiGraf;

            string pot[3];
            pot[0]="grafBig.txt";
            pot[1]="graf.txt";
            pot[2]="grafImena.txt";
            ifstream file(pot[izberiGraf].c_str());

            file>>st_vozlisc;
            file>>st_povezav;

            while(file>>ime1>>ime2>>cena){                imena.push_back(ime1);                imena.push_back(ime2);
                price.push_back(cena);            }

            matrika = new int*[st_vozlisc];//2d dinamicno polje
            for(int i=0; i<st_vozlisc; i++){
                matrika[i]=new int[st_vozlisc];
            }
            for(int i=0; i<st_vozlisc; i++){
                for (int j=0; j<st_vozlisc; j++){
                    matrika[i][j]=0;
                }
            }

            beriGraf(p_poljeVozlisc, st_vozlisc, imena, price, matrika);

            grafNalozen=true;
        }

        else if(izbira==2){
            if(grafNalozen){
                string vozlisce1;
                Vozlisce *s = NULL;
                string vozlisce2;
                Vozlisce *d = NULL;

                while (s==NULL){
                    cout<<"Vpisi zacetno vozlisce: "<<endl;
                    cin>>vozlisce1;
                    s=isciVozlisce(vozlisce1, st_vozlisc, p_poljeVozlisc);
                    if(s==NULL){
                        cout<<"vozlisce "<<vozlisce1<<" ne obstaja!"<<endl;
                    }
                }
                while (d==NULL){
                    cout<<"Vpisi koncno vozlisce: "<<endl;
                    cin>>vozlisce2;
                    d=isciVozlisce(vozlisce2, st_vozlisc, p_poljeVozlisc);
                    if(d==NULL){
                        cout<<"vozlisce "<<vozlisce2<<" ne obstaja!"<<endl;
                    }
                }
               auto start = std::chrono::high_resolution_clock::now();
                iskanje_v_globino_sirino(s, d, matrika, p_poljeVozlisc, st_vozlisc, true);
               auto finish = std::chrono::high_resolution_clock::now();
               std::cout << "Cas iskanja v globino: " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
                //bool globina=true
            }
            else{
                cout<<"Graf se ni nalozen. Nalozi graf."<<endl;
            }
        }

        else if(izbira==3){
            if(grafNalozen){
                string vozlisce1;
                Vozlisce *s = NULL;
                string vozlisce2;
                Vozlisce *d = NULL;

                while (s==NULL){
                    cout<<"Vpisi zacetno vozlisce: "<<endl;
                    cin>>vozlisce1;
                    s=isciVozlisce(vozlisce1, st_vozlisc, p_poljeVozlisc);
                    if(s==NULL){
                        cout<<"vozlisce "<<vozlisce1<<" ne obstaja!"<<endl;
                    }
                }

                while (d==NULL){
                    cout<<"Vpisi koncno vozlisce: "<<endl;
                    cin>>vozlisce2;
                    d=isciVozlisce(vozlisce2, st_vozlisc, p_poljeVozlisc);
                    if(d==NULL){
                        cout<<"vozlisce "<<vozlisce2<<" ne obstaja!"<<endl;
                    }
                }
               auto start = std::chrono::high_resolution_clock::now();
                iskanje_v_globino_sirino(s, d, matrika, p_poljeVozlisc, st_vozlisc, false);
               auto finish = std::chrono::high_resolution_clock::now();
               std::cout << "Cas iskanja v globino: " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
                //bool globina=true
            }
            else{
                cout<<"Graf se ni nalozen. Nalozi graf."<<endl;
            }
        }

        else{
            running = false;
        }
    }while(running);
    return 0;
}
