import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';
import { Produto } from '../../core/models/produto.model';
import { ProdutoService } from '../../core/services/produto.service';

@Component({
  selector: 'app-nota-fiscal-detalhe',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './nota-fiscal-detalhe.html',
})
export class NotaFiscalDetalhe implements OnInit {
  notaId!: string;
  nota = signal<NotaFiscal | null>(null);
  produtos = signal<Produto[]>([]);

  produtoSelecionadoId = '';
  quantidade = 1;

  imprimindo = signal(false);
  mensagemErro: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly notaFiscalService: NotaFiscalService,
    private readonly produtoService: ProdutoService,
  ) {}

  ngOnInit(): void {
    this.notaId = this.route.snapshot.paramMap.get('id')!;
    this.carregarNota();
    this.produtoService.listar().subscribe(produtos => this.produtos.set(produtos));
  }

  carregarNota(): void {
    this.notaFiscalService.listar().subscribe(notas => {
      const encontrada = notas.find(n => n.id === this.notaId) ?? null;
      this.nota.set(encontrada);
    });
  }

  adicionarItem(): void {
    this.mensagemErro = null;
    const produto = this.produtos().find(p => p.id === this.produtoSelecionadoId);
    if (!produto) return;

    this.notaFiscalService.adicionarItem(this.notaId, {
      produtoId: produto.id,
      descricaoProduto: produto.descricao,
      quantidade: this.quantidade,
    }).subscribe({
      next: () => {
        this.quantidade = 1;
        this.carregarNota();
      },
      error: (err) => {
        this.mensagemErro = err?.error?.mensagem ?? 'Erro ao adicionar item.';
      },
    });
  }

  imprimir(): void {
    this.mensagemErro = null;
    this.imprimindo.set(true);

    this.notaFiscalService.imprimir(this.notaId).subscribe({
      next: (notaAtualizada) => {
        this.imprimindo.set(false);
        this.nota.set(notaAtualizada);
      },
      error: (err) => {
        this.imprimindo.set(false);
                this.mensagemErro = err?.error?.mensagem ?? 'Erro ao imprimir nota fiscal.';
      },
    });
  }
}