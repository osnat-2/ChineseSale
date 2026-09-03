import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DonorService } from '../../services/donorService/donor-service';
import { donorModel } from '../../models/donor';

@Component({
  selector: 'app-donor',
  imports: [CommonModule, RouterLink],
  templateUrl: './donor.html',
  styleUrl: './donor.scss',
})
export class Donor{
  
}