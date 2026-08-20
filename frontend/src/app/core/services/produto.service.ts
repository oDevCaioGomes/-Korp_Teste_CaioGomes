import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produto } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly baseUrl = 'http://localhost:5153/api/produtos';

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.baseUrl);
  }

  criar(produto: { codigo: string; descricao: string; saldoInicial: number }): Observable<Produto> {
    return this.http.post<Produto>(this.baseUrl, produto);
  }
}