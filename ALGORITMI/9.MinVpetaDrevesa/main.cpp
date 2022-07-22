#include <iostream>
#include<vector>
#include<deque>
#include<string>
#include<cstdlib>
#include<fstream>
#include<ctime>
#include<cstdlib>
#include<sstream>
#include<chrono>

using namespace std;

struct Vozlisce{
    string ime;
};
struct Povezava{
    int cena;
    bool odobrena;
    string v1;
    string v2;
};

void meni(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "1 ... PREBERI GRAF\n";
	cout << "2 ... GENERIRAJ NAKLJUCNI GRAF\n";
	cout << "3 ... POZENI ALGORITEM\n";
	cout << "4 ... IZPISI SPREJETE POVEZAVE\n";
	cout << "5 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

void preberiGraf(Vozlisce*& p_poljeVozlisc, int st_vozlisc, vector<string> imena){

    Vozlisce* poljeVozlisc=new Vozlisce[st_vozlisc]; // za imena in indekse
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
			k++;
		}
	}
    p_poljeVozlisc=p_vozlisce;//za prenos polja vozlisc
}

void quickSort(vector<Povezava*>&povezave, int levi, int desni){

    int i=levi;
    int j=desni;
    int pivot=povezave[(desni+levi)/2]->cena;

    while(j>=i){
        while(povezave[i]->cena>pivot)
           i++;
        while(povezave[j]->cena<pivot)
           j--;
        if(j>=i){
            swap(povezave[i],povezave[j]);
            i++;
            j--;
        }
    }
    if (levi < j){
        quickSort(povezave, levi, j);//levi del
    }
    if (i < desni){
        quickSort(povezave, i, desni);//desni del
    }
}

void nakljucniGraf(Vozlisce*& p_poljeVozlisc, int st_vozlisc, vector<Povezava*> &povezave){

    Vozlisce* poljeVozlisc=new Vozlisce[st_vozlisc];
    Vozlisce* p_vozlisce = poljeVozlisc;
    for(int i=0; i<st_vozlisc; i++){ //naredi vozlisca
        stringstream s1;
        s1 << i;
        poljeVozlisc[i].ime=s1.str();
       // cout<<poljeVozlisc[i].ime;
    }
    p_poljeVozlisc=p_vozlisce;//za prenos polja vozlisc

    for(int i=0; i<st_vozlisc; i++){ //povezave med temi vozlisci
        stringstream s1;
        s1 << i;
        for(int j=0; j<st_vozlisc-1; j++){
            stringstream s2;
            s2 << j;
            Povezava* povezava = new Povezava();
            if(j==i || j<i){
                continue;
            }
            else{
                povezava->v1=s1.str();
                povezava->v2=s2.str();
                povezava->cena=rand()%150+1;
                povezava->odobrena=false;
            }
            //cout<<" "<<povezava->v1<<" "<<povezava->v2<<" "<<povezava->cena;
            povezave.push_back(povezava);
        }
    }
}

void kurskal(vector<Povezava*> povezave, int st_vozlisc, Vozlisce*& poljeVozlisc){
   int* mnozica = new int[st_vozlisc];
   int zeObstojeca=1;

   for(int i=0; i<st_vozlisc; i++){
        mnozica[i]=0;
   }
   for(int i=povezave.size()-1; i>0; i--){
        int k;
        int j;
        for(int l=0; l<st_vozlisc; l++){
            if(povezave[i]->v1==poljeVozlisc[l].ime){
                k=l;
            }
            if(povezave[i]->v2==poljeVozlisc[l].ime){
                j=l;
            }
        }
       // cout << " k=" << k << " j=" << j << endl;
        if(mnozica[j]==mnozica[k] && mnozica[j]!=0){//nastal bi cikel
           povezave[i]->odobrena=false;
         //  cout <<"Pogoj 1" << endl;
        }
        else if(mnozica[j]!=mnozica[k] && mnozica[j]!=0 && mnozica[k]!=0){//zdruzi 2 mnozici
            int temp=mnozica[j];
            for(int t=0; t<st_vozlisc; t++){
                if(mnozica[t]==temp){ ///prav za graf z imeni
                    mnozica[t]=mnozica[k];
                }
            }
            povezave[i]->odobrena=true;
            //cout <<"Pogoj 2" <<endl;
        }
        else if((mnozica[j]==0 && mnozica[k]!=0) || (mnozica[k]==0 && mnozica[j]!=0)){
            if(mnozica[j]==0){
                mnozica[j]=mnozica[k];
            }
            else if(mnozica[k]==0){
                mnozica[k]=mnozica[j];
            }
            povezave[i]->odobrena=true;
           // cout <<"Pogoj 3" << endl;
        }
        else{
            mnozica[j]=zeObstojeca;
            mnozica[k]=zeObstojeca;
            zeObstojeca++;
            povezave[i]->odobrena=true;
           // cout <<"Pogoj 4" << endl;
        }
       /*for(int z=0; z<st_vozlisc; z++){
            cout<<mnozica[z];
        }
        cout<<endl;*/
    }
}

int main(){

    srand (time(NULL));
    Vozlisce *p_poljeVozlisc = NULL;
    int st_vozlisc;
    int st_povezav;
    string ime1;
    string ime2;
    int cena;
    vector<int> price;
    vector<string> imena;
    int velikost;

    vector<Povezava*> povezave;

    std::chrono::high_resolution_clock::time_point start;
    std::chrono::high_resolution_clock::time_point finish;

    bool random;

    bool grafNalozen=false;
    bool running=true;
    do{
        meni();
        int izbira;
        cin>>izbira;

        if(izbira==1){
            price.clear();
            random=false;

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
            file>>st_povezav;

            while(file>>ime1>>ime2>>cena){                imena.push_back(ime1);                imena.push_back(ime2);
                price.push_back(cena);

                Povezava* povezava = new Povezava();
                povezava->v1=ime1;
                povezava->v2=ime2;
                povezava->cena=cena;
                povezava->odobrena=false;

                povezave.push_back(povezava);            }
            preberiGraf(p_poljeVozlisc, st_vozlisc, imena);

            grafNalozen=true;
        }

        else if(izbira==2){
            random=true;

            cout<<"Vpisi zeleno stevilo vozlisc v grafu."<<endl;
            cin>>velikost;
            nakljucniGraf(p_poljeVozlisc, velikost+1, povezave);
            grafNalozen=true;
        }

        else if(izbira==3){
            if(grafNalozen==true){
                quickSort(povezave , 0, povezave.size()-1);
              /*  for(int i=0; i<povezave.size(); i++){
                    cout<<povezave[i]->cena<<" ";
                }
                */
                cout<<endl;
                if(random==false){
                start = std::chrono::high_resolution_clock::now();
                    kurskal(povezave, st_vozlisc, p_poljeVozlisc);
                finish = std::chrono::high_resolution_clock::now();
                std::cout << "Cas iskanja: " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";

                    cout<<"Stevilo vhodnih vozlisc: "<<st_vozlisc<<endl;
                    cout<<"Stevilo vhodnih povezav: "<<st_povezav<<endl;
                    int st_sprejetih=0;
                    for(int i=0; i<povezave.size(); i++){
                        if(povezave[i]->odobrena==true){
                            st_sprejetih++;
                        }
                    }
                    cout<<"Stevilo sprejetih povezav:"<<st_sprejetih<<endl;
                }
                else{
                start = std::chrono::high_resolution_clock::now();
                    kurskal(povezave, velikost, p_poljeVozlisc);
                finish = std::chrono::high_resolution_clock::now();
                std::cout << "Cas iskanja: " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";

                    cout<<"Stevilo vhodnih vozlisc: "<<velikost<<endl;
                    cout<<"Stevilo vhodnih povezav: "<<(velikost-1)*velikost/2<<endl;
                    int st_sprejetih=0;
                    for(int i=0; i<povezave.size(); i++){
                        if(povezave[i]->odobrena==true){
                            st_sprejetih++;
                        }
                    }
                    cout<<"Stevilo sprejetih povezav:"<<st_sprejetih<<endl;
                }
            }
            else{
                cout<<"Graf ni nalozen."<<endl;
            }
        }

        else if(izbira==4){
            if(grafNalozen){
                cout<<"vozlisce1:  vozlisce2:  razdalja:"<<endl;
                for(int i=0; i<povezave.size(); i++){
                    if(povezave[i]->odobrena==true){
                        cout<<"    "<<povezave[i]->v1<<"\t\t"<<povezave[i]->v2<<"\t   "<<povezave[i]->cena<<endl;
                    }
                }
            }
            else{
                cout<<"Graf ni nalozen."<<endl;
            }
        }
        else{
            running = false;
        }
    }while(running);

    return 0;
}
