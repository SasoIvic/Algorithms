# Edmond-Karp-Algorithm-Max-Flow-Problem
Edmond-Karp Algorithm Max Flow Problem in C++

## Public Functions:
```
Graph g(size) // create new graph g with starting size
addEdge(vertex_a, vertex_b, capacity) //adds directed edge from vertex_a to vertex_b with a capacity
findMaxFlow(source,sink)  //returns max flow as int; prints edges & max flow Path with residual capacity & starting capacity
resize(newsize) // resizes the graph 
```
-Nick Caverly
-Tony Cicero
-Kevin Fretz

# Using Edmond-Karp Algorithm to Solve the Max Flow Problem
**Abstract:** This paper is an introduction into the max flow problem. We implement the Edmonds-Karp algorithm, which executes in O(VE2) time. Max Flow is the term used to describe how much of a material can be passed into a flow network, which can be used to model many real word situations. One of the most common uses for flow networks are to model electrical circuits. 
**Introduction:** The motivation for this project is to find the Max Flow of a given flow network. A flow network is defined as a directed graph G =(V,E), with vertices V and edge set E. For all edges (u,v) there can be no parallel edge (v,u).  Each edge in E also has a given capacity, defined by c(u,v). This capacity must be nonzero, and represents how much of a given resource or material can be passed along that edge. If there exists no edge between u and v, we define c(u,v)=0. The amount currently being passed through any edge at any given time is known as the flow of that edge, defined by f(u,v). The flow of any given edge must be nonnegative and less than the capacity of the edge, that is f(u,v) ≤ c(u,v). This is known as the capacity constraint, and is an integral part of the max flow problem. 
Another important rule of a flow network is known as flow conservation. Flow conservation states that for any vertex u, 
That is to say, the sum of all flows going in to a vertex u is equal to the sum of all flows leaving vertex u, or more simply put, “flow in equals flow out.”
The flow of any flow network G must satisfy both the capacity constraint and the flow conservation property. 
 The residual capacity of a given edge (u,v) is defined as
 cf(u,v) =
The residual capacity represents the amount capable of being passed to the edge without breaking the capacity constraint. Because a flow network is defined not to have parallel edges, each edge will only trigger one of the cases. The residual capacities of each edge are used to create a new directed graph named Gf. This graph is known as the residual network of G. This graph almost fits the definition of a flow network, except that it allows for parallel edges. The residual capacity of (v, u) is exactly equal to (u, v), because it shows how many items passed along the edge are capable of being negated by essentially flowing backwards. This is an important part of the algorithm used to determine the max flow of a flow network. The idea is that if you pass a given amount x of a resource down an edge, and then pass back an amount y along the edge, it is the same as if you had passed x-y down the edge originally. 
An augmenting path p is a simple path from s to t, in the residual network Gf. Based on the definition of the residual network, the flow of any edge (u,v) on an augmenting path may be increased by at most cf(u,v) without violating the capacity constraint. The residual capacity of any path p, or cf(p) is the minimum residual capacity of all edges in the path. This is because of the capacity constraint, requiring that you do not have a flow on any edge greater than that edges capacity, so if you attempted to increase the augmented path by anything greater than the lowest residual capacity on the path, then the capacity constraint is broken. If f is some flow in G, and f’ is the corresponding flow in Gf, then we define the augmentation of flow f by f’ to be a function from V x V to R defined by 
.
The augmentation of flow f by f’ (f ⬆ f’) = | f | + | f’|.
Intrinsically related to the max-flow problem is its dual, which is the min-cut problem. A cut of flow network G = (V, E) is a partition of V into S and T= V- S, such that the source s and tT. The net flow, f(S,T) across the cut (S,T) is defined to be

The net flow of any cut on a flow network is the same as the flow on that network. 
The capacity of the cut (S,T) is

A minimum cut of a network is a cut whose capacity is minimum over all cuts of the network. For capacity, only the edges going from S to T are counted, while the net flow takes into consideration the slow going from S to T as well as the flow from T to S. 





**Topic Details:** The max flow problem was first formulated in 1954 by T.E. Harris and F.S. Ross as a simplified model of Soviet railway traffic flow. IN 1955, Lester R. Ford Jr. and Delbert R. Fulkerson created the first known algorithm for solving the problem. The For-Fulkerson method is relatively simple, and the algorithm we implemented builds upon it and is known as the Edmonds and Karp algorithm. The Ford-Fulkerson method can be stated relatively simply, as;
FORD-FULKERSON-METHOD(G, s, t)
1 initialize flow f to 0
2 while there exists an augmenting path p in the residual network Gf
3 augment flow f along p
4 return f

	The general idea behind this algorithm is to find any path p from s to t in the residual network. This path will begin with flow 0, and the algorithm will look at each edge in the path, and find the edge with the minimum residual capacity. Remember that for any path p, the cf (p), is defined as min{cf (u, v) : (u ,v ) Ep }. Once it has found the residual capacity, it will add this value to the flow of the path, which will conserve both the capacity constraint and the conservation of flow property. The capacity constraint is upheld because the residual capacity added to the flow is at most the residual capacity left on the ledge with the smallest capacity, making the residual capacity on that edge 0, and all other edges must have some cf 0, since the value subtracted from their residual capacity is less than or equal to their residual capacity. The conservation of flow is upheld because the same value is added to each edge of the path, meaning that the increase in input is the same as the increase in output of each vertex. We know that with each augmentation, we get closer to the maximum flow, as (f ⬆ f’ ) = | f  | + | f’ |. Because flows are nonnegative, we know that ( f ⬆ f’ ) | f |. Because the goal is to find the maximum amount of flow capable of being passed into the flow network, this means we are one step closer to our goal. 
The run time of this algorithm depends upon how the path p is chosen. If chosen poorly enough, the Ford-Fulkerson method may never even terminate. However, if you use Breadth First Search (BFS) to determine your augmenting path, then the algorithm executes in polynomial time. The Edmonds-Karp algorithm implements this strategy, and as a result has a runtime of O(VE2).
Using the Edmonds-Karp algorithm, the flow of the network is augmented O(VE) times. To perform an augmentation, we must have some edge (u, v) along path p : cf ( p ) = cf (u,v). We call this edge a critical edge. Because this critical edge is then filled to capacity, it is erased from the residual network. Additionally, at least one edge on any augmenting path must be critical, and each edge in E can become critical at most | V | / 2 times. Since we used the Edmonds-Karp algorithm, we know that the augmenting path is the shortest path from s to t.  Therefore, when  (u, v) is critical for the first time, we know that the distance of the path from (s, v) can be defined as δf(s, v) = δf (s, u) + 1.  Once this flow is augmented, and the path disappears from the residual network, it will not reappear unless the flow from u to v is decreased, which only occurs if (v, u) appears on an augmenting path. If f’ is the flow when this happens, then we have 	
δf’(s, u) = δf’ (s,v) + 1. Since the distance of the minimum path increases with each augmentation, we then have δf (s,v) δf ‘(s,v). 
δf’(s, u) = δf’ (s,v) + 1
	≥ δf (s, v) + 1
	= δf (s, u) + 2.
As a result, in the time between (u, v) being critical the first time and the next time it becomes critical, the distance of u increases by at least 2. The initial distance of u from s is initially at least 0, and because the intermediate vertices on a shortest path from s to u cannot contain s, u, or t, since (u,v) on an augmenting path implies that u  t. Therefore, the max distance that a vertex u can be from s until it becomes unreachable from the source, if ever, its distance is at most |V| - 2. So, after an edge becomes critical once, it can become critical at most (|V| - 2)/2 = |V|/2 – 1 times more, for a total of |V|/2 times. Since there are O(E) pairs of vertices that have edges between them in the residual network, the total number of critical edges during the Edmonds-Karp execution is O(VE), and because we can implement each iteration of Ford-Fulkerson in O(E) time, the total running time is O(VE2). 
But how do we know that this algorithm actually returns the max flow of a flow network? As mentioned earlier, the max flow of a flow network is dually related to the minimum cut of the same flow network. This is known as the Max-Flow min-cut theorem. This theorem states that the following three conditions are equivalent:
    1. f  is a maximum flow in G.
    2. The residual network Gf  contains no augmenting paths. 
    3. | f | = c( S, T) for some cut (S, T) of G.
To prove this theorem, first we prove that condition (1) implies condition (2). Assume for contradiction that f is a max flow in G but that Gf   has some augmenting path p. This would imply that for path p, there exists some c­f that can be added to path p, resulting in a value strictly greater than f. This contradicts the premise that f is a max flow. Next, we show that (2) implies (3). Suppose that Gf has no augmenting path, and define S = {vV: there exists some path from s to v in Gf } and T = V – S. This cut trivially implies sand t is not, because there is no path from s to t. Now assume there is some u and vIf (u, v)then we must have f(u,v) = c(u,v), or else (u, v) Ef, which would place v in set S. If (v, u)  E, we must have f(v, u) = 0, otherwise cf(u,v) = f(v, u) would be positive and (u, v) f  placing v in S. If neither (u,v) or (v, u) are in the edge set, then f(u,v) = f(v, u) = 0. Following this, the net flow across the cut can be shown as such:


			

			       =c(S, T).
Because we know that the flow of any network is equal to the net flow across any cut,
 | f | = f(S,T) = c(S,T).
To prove that (3) implies (1), first we must take into account that the value of any flow f in a flow network G is bounded from above by the capacity of any cut of G. Based on the condition that | f | = c(S,T), we know that f  is a maximum flow. 
Given more background and understanding, we can expand upon the pseudocode of the Ford-Fulkerson method given before, using the Edmonds-Karp strategy of implementing BFS.  
Edmonds-Karp (G, s, t)
1 for each edge (u, v) 
2 	(u, v).f = 0
3 while BFS returns a path from s to t in the residual network Gf
4	cf(p ) = min {cf (u, v) : (u,v) is in  p }
5 	for each edge (u, v) in p
6 		if (u, v) E
7 			(u, v).f = (u, v).f + cf (p)
8 		else (v, u).f = (v, u).f – cf(p)

	The program will also keep track of the current flow, and increase the current flow by each augmentation, and then return this value. This value will be the maximum flow, as proven by the max-flow min-cut theorem. 

**Conclusion:** While the Edmonds-Karp algorithm is not the fastest method of obtaining the max-flow of a flow network, however it is relatively simple to understand, and is only a small addition to BFS along with an expansion on graph theory. This algorithm runs in O(VE2), which is supported by proofs and our own timings. Some faster algorithms such as push-label algorithms, which run in O(V2E) which is faster than Edmonds-Karp method because you will always have more edges than vertices. 

**Appendix:** 

| Edges | Vertices | Run-time |
|-------|----------|----------|
| 10    | 6        | ~0.0005s |
| 24    | 12       | ~0.0021s |
| 28    | 12       | ~0.0040s |
| 32    | 12       | ~0.0064s |


GitHub for our source code: https://github.com/TonyCicero/Edmond-Karp-Algorithm-Max-Flow-Problem
#ifndef MaxFlow_H


#define MaxFlow_H




#include<map>

#include<vector>

#include<iostream>

#include<queue>

#include<limits.h>




using namespace std;







class Graph{

	private:

		int arr_size;

		int num_points;

		enum color_t{

  			WHITE, GRAY, BLACK

		};

	

		struct Edge{

			int capacity;

			int residual;

		};

		Edge **edges;

		//vector<vector<Edge>> edges;




		map<int, std::vector<int>> V;

		map<int, int> parent;

		map<int, int> distance;

		map<int, color_t> color;

		queue<int> Q;	// holds all gray vertices

	

		bool BFS (int s, int t);// returns true if path from s to t

	public:

		void resize(int);

		void addEdge(int,int, int); // add directed edge to the graph,

		int findMaxFlow(int, int);// Ford-Fulkerson Algorithm

		Graph(int n){

			arr_size = n;

			edges = new Edge*[n];

			for (int i = 0; i < n; i++){

				for(int j = 0; j < n; j++){

					edges[i] = new Edge[n];

					edges[i][j].residual = -1;	

				}

			}

			num_points = 0;	

		}

	~Graph(){

		delete edges;	

	}

	

};




#endif

#include "MaxFlow.h"


#include <iostream>

#include <chrono>

#include <time.h>  




using namespace std;




int main(){

	auto start = std::chrono::system_clock::now();

	auto end = std::chrono::system_clock::now();

	std::chrono::duration<double> elapsed_seconds;

	std::time_t end_time; 

	

	Graph g(20);	//create graph with n edges

	

	//addEdge(1st point, 2nd point, capacity)

	

	//models the graph from https://www.geeksforgeeks.org/ford-fulkerson-algorithm-for-maximum-flow-problem/

	g.addEdge(0,1,16);

	g.addEdge(0,2,13);

	g.addEdge(1,2,10);

	g.addEdge(2,1,4);

	g.addEdge(1,3,12);

	g.addEdge(2,4,14);

	g.addEdge(3,2,9);

	g.addEdge(4,3,7);

	g.addEdge(3,5,20);

	g.addEdge(4,5,4);




	








	cout <<"Finding Maximum Flow..." << endl;

	start = std::chrono::system_clock::now();

	cout << "The Maximum Flow is: " << g.findMaxFlow(0,5) << endl;	//find max flow from s to t

	end = std::chrono::system_clock::now();

	elapsed_seconds = end-start;

	end_time = std::chrono::system_clock::to_time_t(end);

	std::cout<< "elapsed time: " << elapsed_seconds.count() << "s\n";

	

}




**Bibliography: **

Introduction to Algorithms (3rd ed.), by Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest and Clifford Stein.
“Ford-Fulkerson Algorithm for Maximum Flow Problem.” GeeksforGeeks, 7 June 2017, www.geeksforgeeks.org/ford-fulkerson-algorithm-for-maximum-flow-problem/.
