#include <iostream>
#include <ctime>
#include <cstdlib>
#include <vector>
#include <algorithm>
#include <chrono>

using namespace std;

void meni(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "0 ... GENERIRAJ NAKLJUCNO ZAPOREDJE\n";
	cout << "1 ... GENERIRAJ NARASCAJOCE ZAPOREDJE\n";
	cout << "2 ... GENERIRAJ PADAJOCE ZAPOREDJE\n";
	cout << "3 ... IZPIS ZAPOREDJA\n";
	cout << "4 ... PREVERI ALI JE UREJENO\n";
	cout << "5 ... UREDI S QUICKSORT BREZ MEDIANE\n";
	cout << "6 ... UREDI S QUICKSORT Z MEDIANO\n";
	cout << "7 ... UREDI Z DRUGIM ALGORITMOM ZA UREANJE(BUBBLE SORT)\n";
	cout << "8 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

bool jeUrejeno(/*double*/int arr[], int dolzina){
    bool narascajoce=false;
    bool padajoce=false;
    bool preveriPadajoce=false;

    if(arr[0]>arr[1])
        preveriPadajoce=true;

    for(int i=0; i<dolzina-1; i++){
        if(arr[i]<=arr[i+1] && preveriPadajoce==false)
            narascajoce=true;
        else if(arr[i]>=arr[i+1] && preveriPadajoce==true)
            padajoce=true;
        else{
            narascajoce=false;
            padajoce=false;
            break;
        }
    }
    if(narascajoce==true || padajoce==true)
        return true;
    else
        return false;
}

void generirajNakljucno(int dolzina, /*double*/int arr[]){
    for(int i=0; i<dolzina; i++){
        arr[i] = rand()%100000;
    }
}
//============= QUICK SORT =============//
void quickSort(/*double*/int arr[], int levi, int desni, bool mediana){

    int i=levi;
    int j=desni;
    int pivot;
    if(mediana==true){
        pivot=arr[(desni+levi)/2];
        mediana=true;
    }
    else{
        pivot=arr[levi];
        mediana=false;
    }

    while(j>=i){
        while(arr[i]<pivot)
           i++;
        while(arr[j]>pivot)
           j--;
        if(j>=i){
            swap(arr[i],arr[j]);
            i++;
            j--;
        }
    }
    //=====DELI=====
    if (levi < j){
        quickSort(arr, levi, j, mediana);//levi del
    }
    if (i < desni){

        quickSort(arr, i, desni, mediana);//desni del
    }
}

int main()
{
    srand(time(NULL));

    int dolzina;
    cout<<"Kako veliko polje stevil zelis generirati?"<<endl;
    cin>>dolzina;
    //double* arr = new double[dolzina];
    int* arr = new int[dolzina];

     bool running=true;
    do{
        int izbira;
        meni();
        cin>>izbira;

//========NAPOLNI POLJE================//
        if(izbira==0){
            generirajNakljucno(dolzina, arr);
        }
        else if(izbira==1){
            for(int i=0; i<dolzina; i++){
                arr[i]=i;
            }
        }
        else if(izbira==2){
            int j=0;
            for(int i=dolzina-1; i>=0; i--){
                arr[j]=i;
                j++;
            }
        }
//========IZPISE POLJE================//
        else if(izbira==3){
            cout<<"Polje stevil: "<<endl;
            for(int i=0; i<dolzina; i++){
                cout<<arr[i]<<" ";
            }
            cout<<endl;
        }
//========PREVERI ÈE JE UREJENO================//
        else if(izbira==4){
            if(dolzina==0)
                cout<<"polje je prazno."<<endl;
            else{
                if(jeUrejeno(arr, dolzina)==true)
                    cout<<"Zaporedje je urejeno."<<endl;
                else
                    cout<<"Zaporedje ni urejeno."<<endl;
            }
        }
 //========QUICK SORT BREZ MEDIANE================//
        else if(izbira==5){
            auto start = std::chrono::high_resolution_clock::now();
            quickSort(arr, 0, dolzina-1, false);
            auto finish = std::chrono::high_resolution_clock::now();
            std::cout << "Cas urejanja (quickSort brez mediane): " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
        }
//========QUICK SORT Z MEDIANO================//
        else if(izbira==6){
            auto start = std::chrono::high_resolution_clock::now();
            quickSort(arr, 0, dolzina-1, true);
            auto finish = std::chrono::high_resolution_clock::now();
            std::cout << "Cas urejanja (quickSort z mediano): " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
        }
//==========BUBBLE SORT====================//
        else if(izbira==7){
          auto start = std::chrono::high_resolution_clock::now();
          for(int i=0; i<dolzina; i++){
            for(int j=i; j<dolzina; j++){
                if(arr[i]>arr[j]){
                    swap(arr[i], arr[j]);
                }
            }
          }
          auto finish = std::chrono::high_resolution_clock::now();
          std::cout << "Cas urejanja (bubble sort): " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";

        }

        else if(izbira==8){
            running = false;
        }
    }while(running);

    return 0;
}
