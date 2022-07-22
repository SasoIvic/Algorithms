#include <iostream>
#include <stdlib.h>
#include <time.h>
#include <windows.h>
#include <vector>

using namespace std;

struct Stevila{
    int num;
    Stevila *prev, *next;
};

int menu(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "1 ... ISKANJE PODATKA\n";
	cout << "2 ... VNOS PODATKA V GLAVO\n";
	cout << "3 ... VNOS PODATKA ZA ELEMENTOM\n";
	cout << "4 ... VNOS PODATKA ZA REPOM\n";
	cout << "5 ... BRISANJE PODATKA\n";
	cout << "6 ... IZPIS SEZNAMA OD GLAVE PROTI REPU\n";
	cout << "7 ... IZPIS SEZNAMA OD REPA PROTI GLAVI\n";
	cout << "8 ... TESTIRAJ HITROST\n";
	cout << "9 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

void dodajNaZacetek(Stevila* &head, int number){

    Stevila *novoStevilo = new Stevila();
    novoStevilo->num=number;
    novoStevilo->next=NULL;
    novoStevilo->prev=NULL;

    if(head==NULL){
        head=novoStevilo;
        return;
    }
    head->prev=novoStevilo;
    novoStevilo->next=head;
    head=novoStevilo;
}

void dodajNaKonec(Stevila* &head, int number){

    Stevila *novoStevilo = new Stevila();
    novoStevilo->num=number;
    novoStevilo->next=NULL;
    novoStevilo->prev=NULL;

    if(head==NULL){
        head=novoStevilo;
        return;
    }
    Stevila *tail=head;
    while(tail->next!=NULL){
        tail=tail->next;
    }
    novoStevilo->prev=tail;
    tail->next=novoStevilo;
}

Stevila* iskanjeStevila(Stevila* &head, int iskani){
    Stevila *current = head;
    while(current!=NULL){
        if(current->num==iskani){
            return current;
        }
        else{
            current=current->next;
        }
    }
    cout<<"Iskanega stevila ni v seznamu!"<<endl;
    return NULL;
}

void dodaj(Stevila* &head, int number, int iskani){

    Stevila *current=iskanjeStevila(head, iskani);

    if(current==NULL){
        return;
    }

    Stevila *novoStevilo = new Stevila();
    novoStevilo->num=number;
    novoStevilo->next=NULL;
    novoStevilo->prev=NULL;

    novoStevilo->prev=current;
    novoStevilo->next=current->next;
    current->next->prev=novoStevilo;
    current->next=novoStevilo;
}

void izpisSeznamaOdGlave(Stevila* &head){
    Stevila* temp=head;
    while(temp!=NULL){
        cout<<temp->num<<" "<<endl;
        temp=temp->next;
    }
}

void izpisSeznamaOdRepa(Stevila* &head){
    Stevila* tail=head;
    while(tail->next!=NULL){
        tail=tail->next;
    }
    while(tail!=NULL){
        cout<<tail->num<<" "<<endl;
        tail=tail->prev;
    }

}

void brisiIzSeznama(Stevila* &head, int iskani){

    Stevila *current=iskanjeStevila(head, iskani);

    if(current==NULL){
        return;
    }
    Stevila *previous=current->prev;
    Stevila *naslednje=current->next;

    previous->next=naslednje;
    naslednje->prev=previous;
}

void hitrost(Stevila* &head){
    int len;
    int vsota=0;
    cout<<"Koliko stevil zelis vnesti v seznam? ";
    cin>>len;

    int kamVstavi;
    cout<<"1 ... vstavljanje v glavo seznama\n2 ... vstavljanje na konec seznama\n3 ...vstavljanje na prvo mesto polja\n4 ... vstavljanje na konec polja"<<endl;
    cin>>kamVstavi;

    if(kamVstavi==1){
        // =========== VSTAVI V GLAVO SEZNAMA ==================== //
        clock_t start = clock();
        for(int i=0; i<len; i++){
            dodajNaZacetek(head, i);
        }
        clock_t finish = clock();        double CasVstavljanjaNaZacetek = (double)(finish - start) / CLOCKS_PER_SEC;
        cout << "Cas vstavljanja  " << len << " elementov na zacetek seznama je " << CasVstavljanjaNaZacetek << endl;
         // =========== VSOTA1 ==================== //
        start = clock();
        Stevila *temp = head;
        long long int vsota = 0;
        while (temp != NULL)
        {
            vsota=vsota+temp->num;
            temp=temp->next;
        }
        finish = clock();        double CasSestevanjaSeznam = (double)(finish - start) / CLOCKS_PER_SEC;
        cout << "Cas sestevanja  " << len << " elementov iz seznama je " << CasSestevanjaSeznam << endl;
    }
    else if(kamVstavi==2){
        // =========== VSTAVI NA KONEC SEZNAMA ============= //
        clock_t start = clock();
        for(int i=0; i<len; i++){
            dodajNaKonec(head, i);
        }
        clock_t finish = clock();
        double CasVstavljanjaNaKonec = (double)(finish - start) / CLOCKS_PER_SEC;
        cout << "Cas vstavljanja  " << len << " elementov na konec seznama je " << CasVstavljanjaNaKonec << endl;
        // =========== VSOTA2 ==================== //
        start = clock();
        Stevila *temp = head;
        long long int vsota2 = 0;
        while (temp != NULL)
        {
            vsota2=vsota2+temp->num;
            temp=temp->next;
        }
        finish = clock();        double CasSestevanjaSeznam2 = (double)(finish - start) / CLOCKS_PER_SEC;
        cout << "Cas sestevanja  " << len << " elementov iz seznama je " << CasSestevanjaSeznam2 << endl;
    }
    else if(kamVstavi==3){
        // =========== VSTAVI NA PRVO MESTO POLJA ============= //
        clock_t start = clock();
        vector<int> polje;
        for(int i=0; i<len; i++){
            polje.insert(polje.begin(),i);
        }
        clock_t finish = clock();
        double naZacetekPolja = (double)(finish - start) / CLOCKS_PER_SEC;
        cout << "Cas vstavljanja  " << len << " elementov na zacetek polja je " << naZacetekPolja << endl;
        /*for(int i=0; i<len; i++){
            cout<<polje[i]<<" ";
        }*/
        cout<<endl;
    }
    else if(kamVstavi==4){
        // =========== VSTAVI NA KONEC POLJA ============= //
        clock_t start = clock();
        int* arr=new int[len];
        for(int i=0;i<len;i++){
            arr[i]=i;
        }
        /*for(int i=0; i<len; i++){
            cout<<arr[i]<<" ";
        }*/
        cout<<endl;
        clock_t finish = clock();
        double naZacetekPolja = (double)(finish - start) / CLOCKS_PER_SEC;
        cout << "Cas vstavljanja  " << len << " elementov na konec polja je " << naZacetekPolja << endl;
        vsota=0;
        for(int i=0; i<len; i++){
            vsota=vsota+arr[i];
        }
    }
    else
        cout<<"izberi 1, 2, 3 ali 4!"<<endl;
}


int main()
{
    srand(time(NULL));
    Stevila *head=NULL;
    char izbira;
    bool running=true;
    do{
        izbira = menu();
        cin>>izbira;
		switch (izbira) {
		    case '1':
		        int iskani;
                cout<<"Vpisi iskano stevilo. ";
                cin>>iskani;
               // Stevila *current=iskanjeStevila(head, iskani);
               // cout<<"Iskano: "<<current->num<<endl;
                break;
            case '2':
                int number;
                cout<<"Vpisi stevilo ki ga zelis vnesti v seznam. ";
                cin>>number;
                dodajNaZacetek(head, number);
                break;
            case '3':
                number;
                cout<<"Vpisi stevilo ki ga zelis vnesti v seznam. ";
                cin>>number;
                cout<<"Vpisi iskano stevilo. ";
                cin>>iskani;
                dodaj(head, number, iskani);
                break;
            case '4':
                cout<<"Vpisi stevilo ki ga zelis dodati vseznam. ";
                cin>>number;
                dodajNaKonec(head, number);
                break;
            case '5':
                cout<<"Vpisi stevilo ki ga zelis izbrisati. ";
                cin>>iskani;
                iskani;
                brisiIzSeznama(head, iskani);
                break;
            case '6':
                izpisSeznamaOdGlave(head);
                break;
            case '7':
                izpisSeznamaOdRepa(head);
                break;
            case '8':
                hitrost(head);
                break;
            case '9':
                running = false;
                break;

            default:
                cout << "Napacna izbira!\n";
                break;
		}
        } while (running);


    return 0;
}
