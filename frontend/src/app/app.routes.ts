import { Routes } from '@angular/router';
import { Produtos } from './features/produtos/produtos';
import { NotasFiscais } from './features/notas-fiscais/notas-fiscais';
import { NotaFiscalDetalhe } from './features/nota-fiscal-detalhe/nota-fiscal-detalhe';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  { path: 'produtos', component: Produtos },
  { path: 'notas-fiscais', component: NotasFiscais },
  { path: 'notas-fiscais/:id', component: NotaFiscalDetalhe },
];