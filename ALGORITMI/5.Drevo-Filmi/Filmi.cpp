#include <iostream>
#include <string>
#include <fstream>
#include <iomanip>
#include <time.h>
#include <windows.h>
#include <time.h>
#include <chrono>

using namespace std;

struct Element{
    string imeFilma;
    Element *next;
};
struct Drevo{
    int num;
    Drevo* oce;
    Drevo* leviSin;
    Drevo* desniSin;
    Element* seznam;
};

Drevo* iskanjeElementa(Drevo* head, int iskano);

void meni(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "0 ... VSTAVLJANJE IZ DATOTEKE\n";
	cout << "1 ... VSTAVI STEVILO\n";
	cout << "2 ... ISCIMO STEVILO\n";
	cout << "3 ... IZPISI DREVO\n";
	cout << "4 ... IZPIS POVEZAV\n";
	cout << "5 ... MINIMUMM\n";
	cout << "6 ... MAKSIMUM\n";
	cout << "7 ... NASLEDNJIK in PREDHODNIK\n";
	cout << "8 ... BRISANJE ELEMENTA\n";
	cout << "9 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

Drevo* dodajElement(Drevo* head, int stevilo, Drevo* oce, Element* glava, string naslovFilma){

    if(head==NULL && oce==NULL && glava==NULL){ // ce je drevo prazno
        Drevo *novoDrevo=new Drevo(); // vstavi nov datum
        novoDrevo->num=stevilo;
        novoDrevo->oce=NULL;
        novoDrevo->leviSin=NULL;
        novoDrevo->desniSin=NULL;

        Element *novElement=new Element(); //vstavi nov element
        novElement->imeFilma=naslovFilma;
        novElement->next=NULL;
        novoDrevo->seznam=novElement; // povezava drevo element

        return novoDrevo;
    }
    else if(head==NULL){ // ko drevo NI prazno
        Drevo *novoDrevo=new Drevo();
        novoDrevo->num=stevilo;
        novoDrevo->oce=oce;
        novoDrevo->leviSin=NULL;
        novoDrevo->desniSin=NULL;

        Element *novElement=new Element(); //vstavi nov element
        novElement->imeFilma=naslovFilma;
        novElement->next=NULL;
        novoDrevo->seznam=novElement; // povezava drevo element

        return novoDrevo;
    }
    else if(stevilo > head->num){ // desni sin(vecji od oceta)
        head->desniSin=dodajElement(head->desniSin, stevilo, head, glava, naslovFilma);
    }
    else if(stevilo < head->num){ // levi sin(manjsi od oceta)
        head->leviSin=dodajElement(head->leviSin, stevilo, head, glava, naslovFilma);
    }
    else if(stevilo == head->num){ // ce se enkrat vstavimo enaki datum
        Element *novElement=new Element(); //vstavi nov element
        novElement->imeFilma=naslovFilma;
        novElement->next=NULL;

        Drevo *enaki;
        enaki=iskanjeElementa(head, head->num); // enaki sedaj kaze na tisto stevilo ki se ponovi
        Element *tail = new Element();
        tail=enaki->seznam;
        while(tail->next!=NULL){ // tail gre èez seznam imen filmov
            tail=tail->next;
        }
        tail->next=novElement;
    }
	return head;
}

Drevo* iskanjeElementa(Drevo* head, int iskano){
    if(head==NULL) // ko je prazno
        return head;
    else if (iskano==head->num)
		return head;
    else{
        if(iskano<head->num)
            iskanjeElementa(head->leviSin, iskano);//gremo levo
        else
            iskanjeElementa(head->desniSin, iskano);//gremo desno
    }
}

void izpisiDrevo(Drevo* head){
    if(head==NULL)//prazno
        return;
    izpisiDrevo(head->leviSin);// idi levo (najprej levo ker je manjse)

    Element *tail = new Element();
    tail=head->seznam;

    cout<<head->num<<": ";
    while(tail!=NULL){
        cout<<tail->imeFilma<<" ";
        tail=tail->next;
    }
    cout<<endl;
    izpisiDrevo(head->desniSin);// idi desno
}

void izpisPovezav(Drevo* head){
    if(head->leviSin!=NULL){
        cout << head->num << "-" << head->leviSin->num << endl;
        izpisPovezav(head->leviSin);
    }
    if(head->desniSin!=NULL){
        cout << head->num << "-" << head->desniSin->num << endl;
        izpisPovezav(head->desniSin);
    }
}

Drevo* minimum(Drevo* head){
    if (head == NULL){ // ce je prazen
		cout << "Seznam je prazen." << endl;
		return NULL;
	}
	else if (head->desniSin == NULL) // ni desnih
		return head;
    else{
        while(head->leviSin!=NULL){ // isce najvecjo
            head=head->leviSin;
        }
        return head;
    }
}
Drevo* maksimum(Drevo* head){
    if (head == NULL){ // ce je prazen
		cout << "Seznam je prazen." << endl;
		return NULL;
	}
	else if (head->leviSin == NULL) // ni levih
		return head;
    else{
        while(head->desniSin!=NULL){ // isce najmanjso
            head=head->desniSin;
        }
        return head;
    }
}

Drevo* predhodnik(Drevo* a){
    if(a->leviSin!=NULL){
        return maksimum(a->leviSin);//najvecji med manjsimi
    }

    Drevo* prejsnji = a->oce;
    while(prejsnji!=NULL && a==prejsnji->leviSin){
        a=prejsnji;
        prejsnji=prejsnji->oce;
    }
    return prejsnji;
}
Drevo* naslednjik(Drevo* a){
    if(a->desniSin!=NULL){
        return minimum(a->desniSin);// najmanjsi med najvecjimi
    }

    Drevo* prejsnji = a->oce;
    while(prejsnji!=NULL && a==prejsnji->desniSin){
        a=prejsnji;
        prejsnji=prejsnji->oce;
    }
    return prejsnji;
}
Drevo* brisanjeElementa(Drevo*& head, int iskano){

    Drevo* izbrisi = iskanjeElementa(head, iskano);

    if(izbrisi!=NULL){
        Drevo* y;
        Drevo* x;

        if(izbrisi->leviSin==NULL || izbrisi->desniSin==NULL)//vozlisce z enim sinovom
            y=izbrisi;
        else // vozlisce z dvema sinovoma
            y=naslednjik(izbrisi);

        if(y->leviSin==NULL)
            x=y->desniSin;
        else
            x=y->leviSin;

        if(x!=NULL)
            x->oce=y->oce;

        if(y->oce==NULL)
            head=x;
        else if(y == y->oce->leviSin)
            y->oce->leviSin=x;
        else
            y->oce->desniSin=x;

        if(y != izbrisi) // korena ne brisemo ampak podatek premaknemo v koren
            izbrisi->num = y->num;

        delete y;
        return y;
    }
    else
        cout<<"Ni stevila za izbris."<<endl;
}

int branjeIzDatoteke(string pot, Drevo* &head, Element* glava){
    ifstream file(pot.c_str());
	int N;
	file >> N;
	cout<<N;

	for (int i=0; i<N; i++){
		int datum;
		file >> datum;

		string ime;
		getline(file, ime, '\n');

        head=dodajElement(head, datum, NULL, glava, ime);

        if(i%1000==0){
            cout << "*"<<std::flush;
        }
	}
}

int main()
{
    Drevo* head=NULL;
    Drevo* iskaniEl=NULL;
    Drevo* a=NULL;

    Element* glava=NULL;
    Element *tail=NULL;

    string naslovFilma;
    int datoteka;
    string pot;

    int iskano;
    char izbira;
    bool running=true;

    std::chrono::high_resolution_clock::time_point start;
    std::chrono::high_resolution_clock::time_point finish;


    do{
        meni();
        cin>>izbira;
		switch (izbira) {
		    case '1':
		        int stevilo; // datum
		        //string naslovFilma;
		        cout<<"Vstavi datum: ";
		        cin>>stevilo;
		        cout<<"Vpisi naslov filma: ";
		        cin>>naslovFilma;
		        head=dodajElement(head, stevilo, NULL, glava, naslovFilma);
                break;

            case '2':
                cout<<"Katero stevilo isces? ";
		        cin>>iskano;

            start = std::chrono::high_resolution_clock::now();
                for(int i=0; i<1000000; i++){
                    iskaniEl=iskanjeElementa(head, iskano);
                }
            finish = std::chrono::high_resolution_clock::now();

            std::cout << "Cas iskanja v drevesu: " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
                if(iskaniEl==NULL){
                    cout<<"Iskanega elementa ni v seznamu."<<endl;
                }
                else{
                    cout<<"Iskani element, "<<iskaniEl->num<<", je v seznamu."<<endl;
                }
                break;

            case '3':
                izpisiDrevo(head);
                cout<<endl;
                break;

            case '4':
                cout<<"Povezave med elementi: "<<endl;
                if(head!=NULL)
                    izpisPovezav(head);
                else
                    cout<<"Seznam je prazen."<<endl;
                break;

            case '5':
                if(head!=NULL)
                    cout<<"minimum: "<<minimum(head)->num<<endl;
                else
                    cout<<"Seznam je prazen."<<endl;
                break;

            case '6':
                if(head!=NULL)
                    cout<<"maxmum: "<<maksimum(head)->num<<endl;
                else
                    cout<<"Seznam je prazen."<<endl;
                break;

            case '7':
                cout << "Vpisi stevilo za iskanje predhodnjika in naslednjika: ";
                cin >> iskano;
                a=iskanjeElementa(head, iskano);
                if(a!=NULL){
                    if(predhodnik(a)==NULL)
                        cout<<"Stevilo nima predhodjika."<<endl;
                    else{
                        cout<< "Predhodnjik je: "<<predhodnik(a)->num<<endl;
                        tail=predhodnik(a)->seznam;

                        while(tail!=NULL){
                            cout<<tail->imeFilma<<" ";
                            tail=tail->next;
                        }
                        cout<<endl;
                    }

                    if(naslednjik(a)==NULL)
                        cout<<"Stevilo nima naslednjika."<<endl;
                    else{
                        cout<< "Naslednjik je: "<<naslednjik(a)->num<<endl;
                        tail=naslednjik(a)->seznam;

                        while(tail!=NULL){
                            cout<<tail->imeFilma<<" ";
                            tail=tail->next;
                        }
                        cout<<endl;
                    }
                }
                else
                    cout<<"Ni tega stevila."<<endl;
                break;

            case '8':
                cout << "Vpisi stevilo ki ga zelis izbrisati: ";
                cin >> iskano;
                brisanjeElementa(head, iskano);
                break;

            case '9':
                running = false;
                break;

             case '0':
                 cout << "Izberi datoteko: " << endl;
                 cout << "  1. IMDB_date_name_full" << endl;
                 cout << "  2. IMDB_date_name_full_sorted" << endl;
                 cout << "  3. IMDB_date_name_mini" << endl;
                 cout << "  4. IMDB_date_name_mini_sorted" << endl;
                 cin>>datoteka;

                 if(datoteka==1){
                    pot="IMDB_date_name_full.list";
                    branjeIzDatoteke(pot, head, glava);
                 }
                 else if(datoteka==2){
                    pot="IMDB_date_name_full_sorted.list";
                    branjeIzDatoteke(pot, head, glava);
                 }
                 else if(datoteka==3){
                    pot="IMDB_date_name_mini.list";
                    branjeIzDatoteke(pot, head, glava);
                 }
                 else{
                    pot="IMDB_date_name_mini_sorted.list";
                    branjeIzDatoteke(pot, head, glava);
                 }

                 break;

            default:
                cout << "Napacna izbira!\n";
                break;
		}
        } while (running);


    return 0;
}
