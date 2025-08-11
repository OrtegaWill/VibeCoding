import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Catalogo, CatalogoCreate } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class CatalogoService {
  private apiUrl = 'http://localhost:5106/api/catalogos';

  constructor(private http: HttpClient) { }

  getCatalogos(tipo?: string): Observable<Catalogo[]> {
    let params = new HttpParams();
    if (tipo) {
      params = params.set('tipo', tipo);
    }
    return this.http.get<Catalogo[]>(this.apiUrl, { params });
  }

  getTiposCatalogo(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/tipos`);
  }

  getCatalogo(id: number): Observable<Catalogo> {
    return this.http.get<Catalogo>(`${this.apiUrl}/${id}`);
  }

  createCatalogo(catalogo: CatalogoCreate): Observable<Catalogo> {
    return this.http.post<Catalogo>(this.apiUrl, catalogo);
  }

  updateCatalogo(id: number, catalogo: CatalogoCreate): Observable<Catalogo> {
    return this.http.put<Catalogo>(`${this.apiUrl}/${id}`, catalogo);
  }

  deleteCatalogo(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
