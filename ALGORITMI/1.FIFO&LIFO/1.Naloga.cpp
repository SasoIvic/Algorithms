
#include <iostream>

using namespace std;

void meni(){
    cout<<"Krozna vrsta-izbira:\n 1. Vnos podatka\n 2. Branje podatka\n 3. Izpis vsebine vrste\n"<<endl;
    cout<<"Sklad-izbira:\n 4. Vnos podatka\n 5. Branje podatka\n 6. Izpis vsebine sklada\n "<<endl;

}

void vnosPodatka(int polje[], int num, int &glava, int &rep, int arraySize){
    if(rep==arraySize){
        cout<<"Prekoracitev velikosti polja."<<endl;
    }
    else{
        cout<<"Vnesi podatek. ";
        cin>>num;
        polje[rep]=num;
        rep++;
    }
}

void izbrisPodatka(int polje[], int num, int &glava, int &rep, int &arraySize){
    if(glava==rep){
        cout<<"Vrsta je ze prazna."<<endl;
    }
    else{
        cout<<polje[glava]<<endl;
        glava++;
        arraySize++;
    }
}

void izpisPolja(int polje[], int &rep, int &glava){
    cout<<"Izpis trenutnega polja: ";
    for(int i=glava; i<rep; i++){
        cout<<polje[i]<<" ";
    }
    cout<<endl<<endl;
}

void vnosPodatkaLIFO(int polje[], int num, int &vrh, int arraySize){
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

void izbrisPodatkaLIFO(int polje[], int num, int &vrh, int arraySize){
    if(vrh==0){
        cout<<"Sklad je ze prazen."<<endl;
    }
    else{
        vrh--;
        cout<<"Zadnja vpisana: "<<polje[vrh]<<endl;
    }
}

void izpisPoljaLIFO(int polje[], int &vrh){
    cout<<"Izpis trenutnega polja: ";
    for(int i=0; i<vrh; i++){
        cout<<polje[i]<<" ";
    }
    cout<<endl<<endl;
}

int main()
{
    int arraySize;
    int num;
    int glava=0;
    int rep=0;
    int vrh=0;
    cout<<"Koliko stevil zelis dodati v polje?"<<endl;
    cin>>arraySize;
    int polje[arraySize];

    int izbira;
    while (izbira != '4'){
        meni();
        cin>>izbira;
        switch(izbira)
        {
            case 1:
                vnosPodatka(polje, num, glava, rep, arraySize);
                break;
            case 2:
                izbrisPodatka(polje, num, glava, rep, arraySize);
                break;
            case 3:
                izpisPolja(polje, rep, glava);
                break;
            case 4:
                vnosPodatkaLIFO(polje, num, vrh, arraySize);
                break;
            case 5:
                izbrisPodatkaLIFO(polje, num, vrh, arraySize);
                break;
            case 6:
                izpisPoljaLIFO(polje, vrh);
                break;
            case 7:
                return 0;
                break;
            default:
                cout<<"Napaka."<<endl;
                break;
        }
    }

    return 0;
}
