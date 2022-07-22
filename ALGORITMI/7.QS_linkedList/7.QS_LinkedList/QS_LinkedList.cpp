#include <iostream>
#include <stdlib.h>
#include <time.h>
#include <chrono>
#include <windows.h>
#include <vector>

using namespace std;

struct Stevila {
	int num;
	int indeks;
	Stevila *prev, *next;
};

void menu() {
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "1 ... GENERIRAJ NAKLJUCNO ZAPOREDJE\n";
	cout << "2 ... GENERIRAJ NARASCAJOCE ZAPOREDJE\n";
	cout << "3 ... IZPIS ZAPOREDJA\n";
	cout << "4 ... PREVERI UREJENOST ZAPOREDJA\n";
	cout << "5 ... IZPISI VSOTO ELEMENTOV\n";
	cout << "6 ... UREDI\n";
	cout << "7 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

void dodajNaKonec(Stevila* &head, Stevila* &tail, int number, int i) {

	Stevila *novoStevilo = new Stevila();
	novoStevilo->num = number;
	novoStevilo->next = NULL;
	novoStevilo->prev = NULL;
	novoStevilo->indeks = i;

	if (head == NULL) {
		head = novoStevilo;
		tail = head;
		return;
	}
	while (tail->next != NULL) {
		tail = tail->next;
	}
	novoStevilo->prev = tail;
	tail->next = novoStevilo;
}

void izpisSeznamaOdGlave(Stevila* head) {
	Stevila* temp = head;
	while (temp != NULL) {
		cout << temp->num << " ";
		temp = temp->next;
	}
	cout << endl;
}

void jeUrejeno(Stevila* head) {
	bool urejeno = false;
	Stevila* temp = head;

	if (head == NULL) {
		cout << "Seznam je prazen." << endl;
	}
	else {
		while (temp->next != NULL) {
			if (temp->num <= temp->next->num)
				urejeno = true;
			else {
				urejeno = false;
				break;
			}
			temp = temp->next;
		}
		if (urejeno == true)
			cout << "Zaporedje je urejeno." << endl;
		else
			cout << "Zaporedje ni urejeno." << endl;
	}
}

long int vsota(Stevila* head) {
	long int sum = 0;
	while (head != NULL) {
		sum = sum + head->num;
		head = head->next;
	}
	return sum;
}

void quickSort(Stevila* head, Stevila* tail) {
	int pivot = head->num;
	Stevila* i = head;
	Stevila* j = tail;

	while (j!=NULL && i!=NULL && j->indeks >= i->indeks) {
		while (i->num < pivot && i->indeks < tail->indeks) {
			i = i->next;
		}
		while (j->num > pivot && j->indeks > head->indeks) {
			j = j->prev;
		}
		if (j->indeks >= i->indeks) {
			swap(j->num, i->num);
			i = i->next;
			j = j->prev;
		}
	}
	if (j!=NULL && i!=NULL) {
		if (head->indeks < j->indeks) {
			quickSort(head, j);//levi del
		}
		if (i->indeks < tail->indeks) {
			quickSort(i, tail);//desni del
		}
	}
}

int main()
{
	srand(time(NULL));
	Stevila *current = NULL;
	Stevila *head = NULL;
	Stevila *tail = NULL;


	int dolzina;
	long int sum;
	std::chrono::high_resolution_clock::time_point start;
    std::chrono::high_resolution_clock::time_point finish;

	char izbira;
	bool running = true;
	do {
		menu();
		cin >> izbira;
		switch (izbira) {
		case '1':
			cout << "Vpisi zeleno dolzino zaporedja." << endl;
			cin >> dolzina;
			for (int i = 0; i<dolzina; i++) {
				dodajNaKonec(head, tail, rand() % 100000, i);
			}
			break;
		case '2':
			cout << "Vpisi zeleno dolzino zaporedja." << endl;
			cin >> dolzina;
			for (int i = 0; i<dolzina; i++) {
				dodajNaKonec(head, tail, i, i);
			}
			break;
		case '3':
			izpisSeznamaOdGlave(head);
			break;
		case '4':
			jeUrejeno(head);
			break;
		case '5':
			sum = vsota(head);
			cout << "Vsota stevil iz seznama: " << sum << endl;
			break;
		case '6':
            start = std::chrono::high_resolution_clock::now();
			quickSort(head, tail->next);
			finish = std::chrono::high_resolution_clock::now();
			std::cout << "Cas urejanja (quickSort brez mediane): " << std::chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count() << "ns\n";
			cout<<"Cas v sekundah: "<<(chrono::duration_cast<std::chrono::nanoseconds>(finish - start).count())/1000000000<<"s"<<endl;
			break;
		case '7':
			running = false;
			break;

		default:
			cout << "Napacna izbira!\n";
			break;
		}
	} while (running);

	return 0;
}
