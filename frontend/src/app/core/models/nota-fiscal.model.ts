export type StatusNotaFiscal = 'Aberta' | 'Fechada';

export interface ItemNotaFiscal {
  produtoId: string;
  descricaoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: string;
  numero: number;
  status: StatusNotaFiscal;
  criadaEm: string;
  quantidadeItens: number;
  itens: ItemNotaFiscal[];
}