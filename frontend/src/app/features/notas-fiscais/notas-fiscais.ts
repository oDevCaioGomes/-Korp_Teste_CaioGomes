import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';

@Component({
  selector: 'app-notas-fiscais',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './notas-fiscais.html',
})
export class NotasFiscais implements OnInit {
  notas = signal<NotaFiscal[]>([]);

  constructor(
    private readonly notaFiscalService: NotaFiscalService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.carregarNotas();
  }

  carregarNotas(): void {
    this.notaFiscalService.listar().subscribe(notas => this.notas.set(notas));
  }

  novaNota(): void {
    this.notaFiscalService.criar().subscribe(nota => {
      this.router.navigate(['/notas-fiscais', nota.id]);
    });
  }
}