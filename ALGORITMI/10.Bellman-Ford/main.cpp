#include<iostream>
#include<string>
#include<cstdlib>
#include<fstream>
#include<vector>
#include<chrono>

using namespace std;

struct Vozlisce{
    int predhodnik;
    int cena_poti;
    int ime;
};

void meni(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "1 ... PREBERI GRAF\n";
	cout << "2 ... GENERIRAJ NAKLJUCNI GRAF\n";
	cout << "3 ... POZENI ALGORITEM\n";
	cout << "4 ... IZPISI NAJKRAJSO POT\n";
	cout << "5 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

void BeriGraf(Vozlisce*& PoljeVozlisc, int st_vozlisc, int** matrika){

    Vozlisce* poljeVozlisc=new Vozlisce[st_vozlisc]; ///ustvari vozlisca
	for (int i = 0; i < st_vozlisc; i++){
		poljeVozlisc[i].cena_poti = 10000;
		poljeVozlisc[i].predhodnik = -1;
		poljeVozlisc[i].ime = i+1;
	}
	PoljeVozlisc = poljeVozlisc;
}

void nakljucniGraf(Vozlisce*& PoljeVozlisc, int st_vozlisc, int** matrika){

    Vozlisce* poljeVozlisc=new Vozlisce[st_vozlisc]; ///ustvari vozlisca
	for (int i = 0; i < st_vozlisc; i++){
		poljeVozlisc[i].cena_poti = 10000;
		poljeVozlisc[i].predhodnik = -1;
		poljeVozlisc[i].ime = i+1;
	}
	PoljeVozlisc = poljeVozlisc;

    for(int i=0; i<st_vozlisc; i++){
        for(int j=0; j<st_vozlisc; j++){
            if (j==i){
				matrika[i][j] = 0; ///vozlisce s sabo ni povezano
			}
			else{
				matrika[i][j] = rand() % 100 + 1; ///nakljucne povezave
			}
        }
    }
}

void izpisiPot(Vozlisce* poljeVozlisc, Vozlisce *s, Vozlisce *d, int cena){
	if(s->ime==d->ime){//enako zacetno in koncno vozlisce
        cout << "Cena poti je " << cena << endl;
        cout<<"Pot je: "<<d->ime<<" ";
	}
    else{
        if (d->predhodnik==-1)
			cout<<"Med "<<s->ime<<" in "<<d->ime<<" ni poti.";
		else{
			izpisiPot(poljeVozlisc, s, &poljeVozlisc[d->predhodnik], cena); ///rekurzivno izpisi pot
			cout<<d->ime<<" ";
		}
    }
}

void Bellman_Ford(int st_vozlisc, Vozlisce*& poljeVozlisc, int** matrika, int s){
	for(int i=0; i<st_vozlisc; i++){ ///vsem vozliscem nastavi vrednosti(zacetek)
		poljeVozlisc[i].cena_poti = 10000;
		poljeVozlisc[i].predhodnik = -1;
	}
	poljeVozlisc[s].cena_poti = 0;///pri zacetnem je cena poti 0
	poljeVozlisc[s].predhodnik = -1;

	for(int i=0; i<st_vozlisc-1; i++){
		for(int j=0; j<st_vozlisc; j++){
			Vozlisce u = poljeVozlisc[j];
			for(int l=0; l<st_vozlisc; l++){
				if(j!=l){///ce vozlisci nista isti
					Vozlisce v = poljeVozlisc[l];
					if(u.cena_poti+matrika[j][l] < v.cena_poti){ ///ali lahko preko vozlisca u pridemo ceneje do vozlisca v kot ce gremo direktno
						poljeVozlisc[l].cena_poti = u.cena_poti + matrika[j][l];
						poljeVozlisc[l].predhodnik = j;
					}
				}
			}
		}
	}
	for(int i=0; i<st_vozlisc; i++){
		Vozlisce u = poljeVozlisc[i];
		for (int j=0; j<st_vozlisc; j++){
			Vozlisce v = poljeVozlisc[j];
			if (u.cena_poti+matrika[i][j] < v.cena_poti){ ///napaka graf vsebuje negativni cikel
			    cout<<"Napaka!"<<endl;
			}
		}
	}
}

int main()
{
    Vozlisce *poljeVozlisc = NULL;
    int **matrika;
    int st_vozlisc;

    int vozlisce1;
    int vozlisce2;

    bool grafNalozen=false;
    bool running=true;
    do{
        meni();
        int izbira;
        cin>>izbira;

        if(izbira==1){
            cout<<"Kateri graf nalozim?"<<endl;
             cout<<"    0...graf1.txt";
             cout<<"    1...graf2.txt";
            int izberiGraf;
            cin>>izberiGraf;

            string pot[3];
            pot[0]="graf1.txt";
            pot[1]="graf2.txt";
            ifstream file(pot[izberiGraf].c_str());

            file>>st_vozlisc;

            matrika = new int*[st_vozlisc];//2d dinamicno polje
            for(int i=0; i<st_vozlisc; i++){
                matrika[i]=new int[st_vozlisc];
            }
            for(int i=0; i<st_vozlisc; i++){
                for (int j=0; j<st_vozlisc; j++){
                    matrika[i][j]=0;
                }
            }
            for (int i = 0; i < st_vozlisc; i++){
                for (int j = 0; j < st_vozlisc; j++){
                    file >> matrika[i][j];
                }
            }
            BeriGraf(poljeVozlisc, st_vozlisc, matrika);

            /*cout<<"MATRIKA:"<<endl;
            for(int i=0; i<st_vozlisc; i++){
                for(int j=0; j<st_vozlisc; j++){
                    cout<<matrika[i][j]<<"\t";
                }
                cout<<endl;
            }*/

            grafNalozen=true;
        }

        else if(izbira==2){
            cout<<"Izberi zeleno stevilo vozlisc."<<endl;
            cin>>st_vozlisc;

            matrika = new int*[st_vozlisc];
            for(int i=0; i<st_vozlisc; i++){
                matrika[i]=new int[st_vozlisc];
            }
            for(int i=0; i<st_vozlisc; i++){
                for (int j=0; j<st_vozlisc; j++){
                    matrika[i][j]=0;
                }
            }

            nakljucniGraf(poljeVozlisc, st_vozlisc, matrika);

            /*cout<<"MATRIKA:"<<endl;
            for(int i=0; i<st_vozlisc; i++){
                for(int j=0; j<st_vozlisc; j++){
                    cout<<matrika[i][j]<<"\t";
                }
                cout<<endl;
            }*/
            grafNalozen=true;
        }

        else if(izbira==3){
            if(grafNalozen){
                cout<<"Vpisi zacetno vozlisce: "<<endl;
                cin>>vozlisce1;
            auto start = std::chrono::high_resolution_clock::now();
                Bellman_Ford(st_vozlisc, poljeVozlisc, matrika, vozlisce1-1);
            auto finish = std::chrono::high_resolution_clock::now();
            std::cout << "Cas iskanja najkrajse poti: " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
            }
            else{
                cout<<"Graf se ni nalozen."<<endl;
            }
        }

        else if(izbira==4){
            if(grafNalozen){
                cout << "Vnesi koncno vozlisce: ";
                cin >> vozlisce2;

                izpisiPot(poljeVozlisc, &poljeVozlisc[vozlisce1-1], &poljeVozlisc[vozlisce2-1], poljeVozlisc[vozlisce2-1].cena_poti);
                cout<<endl;
            }
            else{
                cout<<"Graf se ni nalozen."<<endl;
            }
        }
    }while(running);

    return 0;
}
