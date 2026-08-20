import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, Subject, switchMap, startWith } from 'rxjs';
import { Produto } from '../../core/models/produto.model';
import { ProdutoService } from '../../core/services/produto.service';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './produtos.html',
})
export class Produtos implements OnInit {
  produtos$!: Observable<Produto[]>;
  mensagemErro: string | null = null;

  novoCodigo = '';
  novaDescricao = '';
  novoSaldo = 0;

  // Emite um "sinal" toda vez que a lista precisa ser recarregada
  // (ex: logo depois de cadastrar um produto novo).
  private recarregar$ = new Subject<void>();

  constructor(private readonly produtoService: ProdutoService) {}

  ngOnInit(): void {
    this.produtos$ = this.recarregar$.pipe(
      startWith(undefined),
      switchMap(() => this.produtoService.listar())
    );
  }

  salvarProduto(): void {
    this.mensagemErro = null;

    this.produtoService.criar({
      codigo: this.novoCodigo,
      descricao: this.novaDescricao,
      saldoInicial: this.novoSaldo,
    }).subscribe({
      next: () => {
        this.novoCodigo = '';
        this.novaDescricao = '';
        this.novoSaldo = 0;
        this.recarregar$.next();
      },
      error: (err) => {
        this.mensagemErro = err?.error?.mensagem ?? 'Erro ao salvar produto.';
      },
    });
  }
}