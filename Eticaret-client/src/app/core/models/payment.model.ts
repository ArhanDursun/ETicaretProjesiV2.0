export interface PaymentRequest {
  cardHolderName: string;
  cardNumber: string;
  expireMonth: string;
  expireYear: string;
  cvc: string;
  price: number;
  paymentType: number;
  buyerName: string;
  buyerSurname: string;
  buyerEmail: string;
  buyerGsmNumber: string;
  buyerIdentityNumber: string;
  city: string;
  country: string;
  addressDescription: string;
  zipCode: string;
}

export interface PaymentResponse {
  message: string;
  transactionId?: string;
}
