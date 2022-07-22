#include <iostream>

using namespace std;

void meni(){
    cout<<"Sklad-izbira:\n 1. Vnos podatka\n 2. Branje podatka\n 3. Izpis vsebine sklada\n 4. Zapri program"<<endl;
}

void vnosPodatka(int polje[], int num, int &vrh, int arraySize){
    if(vrh>=arraySize){
        cout<<"Prekoracitev velikosti polja."<<endl;
    }
    else{
        cout<<"Vnesi podatek. ";
        cin>>num;
        polje[vrh]=num;
        vrh++;
    }
}

void izbrisPodatka(int polje[], int num, int &vrh, int arraySize){
    if(vrh==0){
        cout<<"Sklad je ze prazen."<<endl;
    }
    else{
        vrh--;
        cout<<"Zadnja vpisana: "<<polje[vrh]<<endl;
    }
}

void izpisPolja(int polje[], int &vrh){
    cout<<"Izpis trenutnega polja: ";
    for(int i=0; i<vrh; i++){
        cout<<polje[i]<<" ";
    }
    cout<<endl;
}

int main()
{
    int arraySize;
    int num;
    int vrh=0;
    cout<<"Koliko stevil zelis dodati v polje?"<<endl;
    cin>>arraySize;
    int polje[arraySize];

    int izbira;
    while (izbira != '4'){
        meni();
        cout<<"Ali zelite dodati element(1), izbrisati element(2) ali izpisati vsebino sklada(3)?"<<endl;
        cin>>izbira;
        switch(izbira)
        {
            case 1:
                vnosPodatka(polje, num, vrh, arraySize);
                break;
            case 2:
                izbrisPodatka(polje, num, vrh, arraySize);
                break;
            case 3:
                izpisPolja(polje, vrh);
                break;
            case 4:
                return 0;
                break;
            default:
                cout<<"Napaka."<<endl;
                break;
        }
    }

    return 0;
}
