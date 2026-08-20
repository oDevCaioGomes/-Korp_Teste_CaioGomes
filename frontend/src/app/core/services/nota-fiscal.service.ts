import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private readonly baseUrl = 'http://localhost:5010/api/notas-fiscais';

  constructor(private readonly http: HttpClient) {}

  listar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.baseUrl);
  }

  criar(): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.baseUrl, {});
  }

  adicionarItem(notaId: string, item: { produtoId: string; descricaoProduto: string; quantidade: number }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${notaId}/itens`, item);
  }

  imprimir(notaId: string): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.baseUrl}/${notaId}/imprimir`, {});
  }
}