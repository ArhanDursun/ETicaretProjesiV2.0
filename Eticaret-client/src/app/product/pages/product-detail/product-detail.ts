import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  Inject,
  OnInit,
  PLATFORM_ID,
} from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Product } from '../../services/product';
import { OfferService } from '../../services/offer';
import { FormsModule } from '@angular/forms';
import { BasketService } from '../../../basket/services/basket';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Favorite } from '../../../services/favorite';
import { OrderService } from '../../../order/services/order';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ProductDetail implements OnInit {
  product: any = null;
  isLoading: boolean = true;
  showOfferModal: boolean = false;
  offerPrice: number = 0;
  isSubmittingOffer: boolean = false;
  currentUserId: string | null = null;
  selectedQuantity: number = 1;
  offerQuantity: number = 1;
  activeTab: 'comments' | 'questions' = 'comments';
  isFavorited: boolean = false;
  comments: any[] = [];
  questions: any[] = [];
  newComment = { content: '', starCount: 5 };
  newQuestion = { content: '' };
  isSubmittingInteraction: boolean = false;
  hoveredStar: number = 0;
  averageRating: number = 0;
  selectedFilter: number = 0;
  canComment: boolean = false;

  constructor(
    private offerService: OfferService,
    private route: ActivatedRoute,
    private productService: Product,
    @Inject(PLATFORM_ID) private platformId: Object,
    private cdr: ChangeDetectorRef,
    private basketService: BasketService,
    private favoriteService: Favorite,
    private orderService: OrderService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.getCurrentUserId();
    this.route.paramMap.subscribe((params) => {
      const productId = params.get('id');
      if (productId) {
        this.productService.getProductById(productId).subscribe({
          next: (data: any) => {
            this.product = {
              ...data,
              images: data.images && data.images.length > 0
                ? data.images.map((img: string) => `https://localhost:7185${img}`)
                : [],
            };
            this.loadComments(this.product.id);
            this.loadQuestions(this.product.id);
            if (this.currentUserId) {
              this.favoriteService.checkFavoriteStatus(productId).subscribe((status) => {
                this.isFavorited = status;
                this.cdr.detectChanges();
              });
            }
            this.checkPurchaseStatus(productId);
            this.isLoading = false;
            this.cdr.detectChanges();
          },
          error: () => {
            this.isLoading = false;
          },
        });
      }
    });
  }

  increaseQuantity() {
    const currentStock = Number(this.product.stockQuantity);
    if (this.selectedQuantity < currentStock) {
      this.selectedQuantity++;
    } else {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.STOCK_LIMIT'));
    }
  }

  decreaseQuantity() {
    if (this.selectedQuantity > 1) {
      this.selectedQuantity--;
    }
  }

  getCurrentUserId() {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['nameid'] || payload['sub'];
      } catch (error) {}
    }
  }

  openOfferModal() {
    this.showOfferModal = true;
  }

  closeOfferModal() {
    this.showOfferModal = false;
    this.offerPrice = 0;
  }

  submitOffer() {
    if (!this.offerPrice || this.offerPrice <= 0) {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.INVALID_PRICE'));
      return;
    }
    if (this.offerPrice >= this.product.price) {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.OFFER_PRICE_LOW'));
      return;
    }
    this.isSubmittingOffer = true;
    this.offerService.makeOffer(this.product.id, this.offerPrice, this.offerQuantity).subscribe({
      next: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.OFFER_SUCCESS'));
        this.closeOfferModal();
        this.isSubmittingOffer = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert(`${this.translate.instant('WALLET.TOPUP.MESSAGES.ERROR_PREFIX')} ${err.error?.Message || ''}`);
        this.isSubmittingOffer = false;
      },
    });
  }

  addToBasket(product: any) {
    const acceptedOffer = product.offers?.find((o: any) => o.status === 1);
    const finalQuantity = acceptedOffer ? acceptedOffer.quantity : this.selectedQuantity;
    const finalPrice = acceptedOffer ? acceptedOffer.offeredPrice : product.price;
    const dto = {
      productId: product.id,
      quantity: finalQuantity,
      unitPrice: finalPrice,
    };
    this.basketService.addItemToBasket(dto).subscribe({
      next: () => {
        alert(acceptedOffer 
          ? this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.OFFER_BASKET_SUCCESS') 
          : this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.ADD_BASKET_SUCCESS'));
        this.basketService.updateCartCount();
        this.product.stockQuantity -= finalQuantity;
        this.selectedQuantity = 1;
        this.cdr.detectChanges();
      },
      error: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.ADD_BASKET_ERROR'));
        this.cdr.detectChanges();
      },
    });
  }

  switchTab(tab: 'comments' | 'questions') {
    this.activeTab = tab;
  }

  loadComments(productId: string) {
    this.productService.getComments(productId).subscribe({
      next: (res: any) => {
        this.comments = res;
        this.calculateAverage();
        this.cdr.detectChanges();
      },
      error: () => {},
    });
  }

  loadQuestions(productId: string) {
    this.productService.getQuestions(productId).subscribe({
      next: (res: any) => {
        this.questions = res;
        this.cdr.detectChanges();
      },
      error: () => {},
    });
  }

  submitContent() {
    if (!this.newComment.content || this.newComment.content.trim().length < 5) {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.COMMENT_MIN_LENGTH'));
      return;
    }
    this.isSubmittingInteraction = true;
    const data = {
      productId: this.product.id,
      content: this.newComment.content,
      starCount: this.newComment.starCount,
    };
    this.productService.addComment(data).subscribe({
      next: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.COMMENT_SUCCESS'));
        this.newComment.content = '';
        this.newComment.starCount = 5;
        this.loadComments(this.product.id);
        this.isSubmittingInteraction = false;
        this.cdr.detectChanges();
      },
      error: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.COMMENT_ERROR'));
        this.isSubmittingInteraction = false;
      },
    });
    this.cdr.detectChanges();
  }

  submitQuestion() {
    if (!this.newQuestion.content || this.newQuestion.content.trim().length < 5) {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.QUESTION_MIN_LENGTH'));
      return;
    }
    this.isSubmittingInteraction = true;
    const data = {
      productId: this.product.id,
      questionContent: this.newQuestion.content,
    };
    this.productService.addQuestion(data).subscribe({
      next: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.QUESTION_SUCCESS'));
        this.newQuestion.content = '';
        this.loadQuestions(this.product.id);
        this.isSubmittingInteraction = false;
        this.cdr.detectChanges();
      },
      error: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.QUESTION_ERROR'));
        this.isSubmittingInteraction = false;
      },
    });
  }

  onStarHover(event: MouseEvent, index: number) {
    const rect = (event.target as HTMLElement).getBoundingClientRect();
    const isHalf = event.clientX - rect.left < rect.width / 2;
    this.hoveredStar = isHalf ? index - 0.5 : index;
  }

  onStarClick(event: MouseEvent, index: number) {
    const rect = (event.target as HTMLElement).getBoundingClientRect();
    const isHalf = event.clientX - rect.left < rect.width / 2;
    this.newComment.starCount = isHalf ? index - 0.5 : index;
  }

  getStarFill(index: number): number {
    const value = this.hoveredStar > 0 ? this.hoveredStar : this.newComment.starCount;
    if (value >= index) return 100;
    if (value === index - 0.5) return 50;
    return 0;
  }

  submitAnswer(question: any) {
    if (!question.replyText || question.replyText.trim().length < 5) {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.REPLY_MIN_LENGTH'));
      return;
    }
    this.isSubmittingInteraction = true;
    const data = {
      questionId: question.id,
      answerContent: question.replyText,
    };
    this.productService.answerQuestion(data).subscribe({
      next: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.REPLY_SUCCESS'));
        this.loadQuestions(this.product.id);
        this.isSubmittingInteraction = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isSubmittingInteraction = false;
      },
    });
  }

  calculateAverage() {
    if (this.comments.length === 0) {
      this.averageRating = 0;
      return;
    }
    const sum = this.comments.reduce((acc, cur) => acc + cur.starCount, 0);
    this.averageRating = Number((sum / this.comments.length).toFixed(1));
  }

  setFilter(star: number) {
    this.selectedFilter = star;
  }

  getfilteredComments() {
    if (this.selectedFilter == 0) return this.comments;
    return this.comments.filter((c) => {
      const starGroup = Math.max(1, Math.floor(c.starCount));
      return starGroup === this.selectedFilter;
    });
  }

  getCommentCountByStar(star: number) {
    return this.comments.filter((c) => {
      const starGroup = Math.max(1, Math.floor(c.starCount));
      return starGroup === star;
    }).length;
  }

  onToggleFavorite(productId: string) {
    if (!this.currentUserId) {
      alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.FAVORITE_LOGIN'));
      return;
    }
    this.favoriteService.toggleFavorite(productId).subscribe({
      next: (response: any) => {
        this.isFavorited = response.isFavorited;
        alert(response.message);
        this.cdr.detectChanges();
      },
      error: () => {
        alert(this.translate.instant('PRODUCT_DETAIL.INTERACTION_MESSAGES.FAVORITE_ERROR'));
      },
    });
  }

  checkPurchaseStatus(productId: string) {
    if (this.currentUserId) {
      this.orderService.checkPurchaseStatus(productId).subscribe({
        next: (res) => {
          this.canComment = res;
          this.cdr.detectChanges();
        },
        error: () => {
          this.canComment = false;
          this.cdr.detectChanges();
        },
      });
    }
  }
}
