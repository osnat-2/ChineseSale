import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { donorModel } from '../../models/donor';
import { DonorService } from '../../services/donorService/donor-service';
import { CardService } from '../../services/cardService/card-service';
import { cardModel } from '../../models/card';
import { UserService } from '../../services/userService/user-service';
// import { CartService } from '../../services/cartService/cart-service';
import { RouterLink } from '@angular/router';

// Payment Component - Process ticket purchases and buyer information
@Component({
  selector: 'app-payment',
  imports: [CommonModule, RouterLink],
  templateUrl: './payment.html',
  styleUrl: './payment.scss',
})
export class Payment{
  
}